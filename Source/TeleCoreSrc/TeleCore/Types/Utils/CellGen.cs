using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace TeleCore.Unsorted;

public static class CellGen
{
    public static IEnumerable<IntVec3> SectorCells_Old(IntVec3 center, Map map, float radius, float angle,
        float rotation, bool useCenter = false, Predicate<IntVec3> validator = null)
    {
        var cellCount = GenRadial.NumCellsInRadius(radius);
        var startCell = useCenter ? 0 : 1;
        var angleMin = (rotation - angle * 0.5f).AngleWrapped();
        var angleMax = (angleMin + angle).AngleWrapped();
        for (var i = startCell; i < cellCount; i++)
        {
            var cell = GenRadial.RadialPattern[i] + center;
            var curAngle = (cell.ToVector3Shifted() - center.ToVector3Shifted()).AngleFlat();
            var invert = angleMin > angleMax;
            var flag = invert
                ? curAngle >= angleMin || curAngle <= angleMax
                : curAngle >= angleMin && curAngle <= angleMax;
            if ((map != null && !cell.InBounds(map)) || !flag || (validator != null && !validator(cell)))
                continue;
            yield return cell;
        }
    }

    /// <summary>
    ///     Calculates a circular sector of <see cref="IntVec3" /> cells. Recommended to cache the result.
    /// </summary>
    /// <param name="center">Center of circle.</param>
    /// <param name="map">Map reference.</param>
    /// <param name="radius">Radius of circle.</param>
    /// <param name="angle">Angle size of the sector.</param>
    /// <param name="rotation">Rotation of the sector, 0 would make the sector "go" upwards.</param>
    /// <param name="useCenter">Uses the center cells as part of the sector.</param>
    /// <param name="validator">Additional validator to exclude or include cells.</param>
    public static IEnumerable<IntVec3> SectorCells(IntVec3 center, Map map, float radius, float desiredAngle,
        float offset, bool useCenter = false, Predicate<IntVec3> validator = null)
    {
        var startCell = useCenter ? 0 : 1;
        var sectorRadiusSquared = radius * radius;
        var cellCount = GenRadial.NumCellsInRadius(radius);

        //
        var half = desiredAngle * 0.5f;
        var min = (offset - half).AngleWrapped();
        var max = (offset + half).AngleWrapped();
        var invert = min > max;

        //
        for (var i = startCell; i < cellCount; i++)
        {
            var cell = GenRadial.RadialPattern[i] + center;
            if (!cell.InBounds(map) || (validator != null && !validator(cell))) continue;
            var dx = cell.x - center.x;
            var dz = cell.z - center.z;
            var distanceSquared = dx * dx + dz * dz;

            if (!(distanceSquared <= sectorRadiusSquared)) continue;

            var angle = Mathf.Atan2(dx, dz) * Mathf.Rad2Deg;
            angle = angle.AngleWrapped();
            if (invert ? angle >= min || angle <= max : angle >= min && angle <= max) yield return cell;
        }
    }

    public static CellRect ToCellRect(this List<IntVec3> cells)
    {
        var minZ = cells.Min(c => c.z);
        var maxZ = cells.Max(c => c.z);
        var minX = cells.Min(c => c.x);
        var maxX = cells.Max(c => c.x);
        var width = maxX - (minX - 1);
        var height = maxZ - (minZ - 1);
        return new CellRect(minX, minZ, width, height);
    }

    public static IEnumerable<IntVec3> CellsAdjacent8Way(this IntVec3 loc, bool andInside = false)
    {
        if (andInside) yield return loc;

        var center = loc;
        var minX = center.x - (1 - 1) / 2 - 1;
        var minZ = center.z - (1 - 1) / 2 - 1;
        var maxX = minX + 1 + 1;
        var maxZ = minZ + 1 + 1;
        for (var i = minX; i <= maxX; i++)
        for (var j = minZ; j <= maxZ; j++)
            yield return new IntVec3(i, 0, j);
    }

    /// <summary>
    ///     Offsets a list of cells by a given reference cell.
    /// </summary>
    public static List<IntVec3> OffsetIntvecs(IEnumerable<IntVec3> cells, IntVec3 reference)
    {
        var offsetVecs = new List<IntVec3>();
        foreach (var c in cells) offsetVecs.Add(c - reference);

        return offsetVecs;
    }

    /// <summary>
    /// </summary>
    public static Vector3[] CornerVec3s(this IntVec3 origin)
    {
        var originVec = origin.ToVector3();
        return new[]
        {
            originVec, //00
            originVec + new Vector3(1, 0, 0), //10
            originVec + new Vector3(0, 0, 1), //01
            originVec + new Vector3(1, 0, 1) //11
        };
    }

    /// <summary>
    /// </summary>
    public static Vector2[] CornerVecs(this IntVec3 origin)
    {
        var originVec = origin.ToIntVec2.ToVector2();
        return new[]
        {
            originVec, originVec + new Vector2(1, 0),
            originVec + new Vector2(1, 1), originVec + new Vector2(0, 1)
        };
    }

    public static IntVec3 LastPointOnLineOfSightWithHeight(Vector3 start, Vector3 end, float maxHeight,
        Func<IntVec3, bool> validator, bool skipFirstCell = false)
    {
        foreach (var intVec in PointsOnLineOfSightWithHeight(start, end, maxHeight))
            if ((!skipFirstCell || !(intVec.ToVector3() == start.Yto0())) && !validator(intVec))
                return intVec;
        return IntVec3.Invalid;
    }

    /// <summary>
    /// </summary>
    public static IEnumerable<IntVec3> PointsOnLineOfSightWithHeight(Vector3 start, Vector3 end, float maxHeight)
    {
        bool sideOnEqual;

        var startY = start.y;
        var endY = end.y;
        start = start.Yto0();
        end = end.Yto0();

        if (global::TeleCore.TMath.Abs(start.x - end.x) < 0.015625)
            sideOnEqual = start.z < end.z;
        else
            sideOnEqual = start.x < end.x;

        //
        var dx = Mathf.Abs(end.x - start.x);
        var dz = Mathf.Abs(end.z - start.z);

        //
        var x = start.x;
        var z = start.z;

        //
        var i = 1 + dx + dz;
        var x_inc = end.x > start.x ? 1 : -1;
        var z_inc = end.z > start.z ? 1 : -1;

        var error = dx - dz;
        dx *= 2;
        dz *= 2;

        var c = default(IntVec3);
        while (i > 1)
        {
            c.x = (int)x;
            c.z = (int)z;

            var curY = Mathf.Lerp(startY, endY, (c.x - start.x) * 2 / dx);

            if (curY <= maxHeight)
                yield return c;

            if (error > 0 || (error == 0 && sideOnEqual))
            {
                x += x_inc;
                error -= dz;
            }
            else
            {
                z += z_inc;
                error += dx;
            }

            var num = (int)(i - 1);
            i = num;
        }
    }
}