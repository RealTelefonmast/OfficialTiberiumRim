using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using Verse;

namespace TeleCore.Unsorted;

/// <summary>
///     The config for the IO cells around a network structure.
///     <para>The IO modes are noted in<see cref="IOUtils"/></para>
/// </summary>
public class NetIOConfig : Editable
{
    [Unsaved(false)]
    public IntVec2 patternSize = IntVec2.Invalid;
    
    public List<IOCellPrototype> cellsEast;
    public List<IOCellPrototype> cellsNorth;
    public List<IOCellPrototype> cellsSouth;
    public List<IOCellPrototype> cellsWest;

    public List<IOCellPrototype> cellsVisual;

    //
    public string pattern;

    public List<IOCellPrototype> CellsFor(Rot4 rot)
    {
        if (rot == Rot4.North)
            return cellsNorth;
        if (rot == Rot4.East)
            return cellsEast;
        if (rot == Rot4.South)
            return cellsSouth;

        return cellsWest;
    }

    public List<IOCellPrototype> GetCellsFor(Rot4 rotation)
    {
        if (rotation == Rot4.North)
            return cellsNorth;
        if (rotation == Rot4.East)
            return cellsEast;
        if (rotation == Rot4.South)
            return cellsSouth;

        return cellsWest;
    }

    private static void Rotate(List<IOCellPrototype> reference, ref List<IOCellPrototype> toRotate)
    {
        toRotate = new List<IOCellPrototype>();
        foreach (var cell in reference)
        {
            var asVec3 = cell.offset.ToVector3();
            Quaternion rotation = Quaternion.AngleAxis(90, Vector3.up);
            var rotated = rotation * asVec3;
            rotated = new Vector3(Mathf.RoundToInt(rotated.x), 0, Mathf.RoundToInt(rotated.z));
            
            toRotate.Add(new IOCellPrototype
            {
                offset = rotated.ToIntVec3(),
                direction = cell.direction.Rotated(RotationDirection.Clockwise),
                mode = cell.mode
            });
        }
    }

    public void PostLoadCustom(ThingDef def)
    {
        if (pattern != null)
        {
            pattern = Regex.Replace(pattern, @"[\s|]+", "");
        }
        
        if (patternSize.IsValid)
        {
            TLog.Warning("Pattern size should not be set manually.");
        }
        
        if (patternSize.IsInvalid && def != null)
        {
            patternSize = def.size + new IntVec2(2, 2);
            if (pattern != null && pattern.Length != patternSize.Area)
            {
                TLog.Warning($"Pattern is not the same size as the area of the thing ({pattern.Length} != {patternSize.Area}), this will cause an error.");
            }
        }

        if (pattern != null || (cellsNorth == null && cellsEast == null && cellsSouth == null && cellsWest == null))
        {
            cellsNorth = IOUtils.GenerateFromPattern(pattern, patternSize);
        }

        //var cells = cellsNorth ?? cellsEast ?? cellsSouth ?? cellsWest;
        // if (cellsNorth == null) 
        //     Rotate(cells, ref cellsNorth, Rot4.North);

        if (cellsEast == null) 
            Rotate(cellsNorth, ref cellsEast);

        if (cellsSouth == null) 
            Rotate(cellsEast, ref cellsSouth);

        if (cellsWest == null) 
            Rotate(cellsSouth, ref cellsWest);
    }
}