using System.Collections.Generic;
using RimWorld.Planet;
using UnityEngine;

namespace TeleCore.Types;

public class SimpleWorldGrid
{
    private const int SubdivisionsCount = 10;
    public const float PlanetRadius = 100f;
    public const int ElevationOffset = 8192;
    public const int TemperatureOffset = 300;
    public const float TemperatureMultiplier = 10f;
    private static List<int> tmpNeighbors = new();
    public float averageTileSize;

    private int cachedTraversalDistance = -1;
    private int cachedTraversalDistanceForEnd = -1;
    private int cachedTraversalDistanceForStart = -1;
    public List<int> tileIDToNeighbors_offsets;
    public List<int> tileIDToNeighbors_values;
    public List<int> tileIDToVerts_offsets;
    public List<Tile> tiles = new();
    public List<Vector3> verts;
    public float viewAngle;
    public Vector3 viewCenter;

    public SimpleWorldGrid()
    {
        CalculateViewCenterAndAngle();
        PlanetShapeGenerator.Generate(10, out verts, out tileIDToVerts_offsets, out tileIDToNeighbors_offsets,
            out tileIDToNeighbors_values, 100f, viewCenter, viewAngle);
        CalculateAverageTileSize();
    }

    public float PlanetCoverage => 1.0f;

    public int TilesCount => tileIDToNeighbors_offsets.Count;
    public Vector3 NorthPolePos => new(0f, 100f, 0f);
    public bool HasWorldData => false; //this.tileBiome != null;

    //
    public Tile this[int tileID]
    {
        get
        {
            if ((ulong)tileID >= (ulong)TilesCount) return null;
            return tiles[tileID];
        }
    }

    private void CalculateViewCenterAndAngle()
    {
        viewAngle = PlanetCoverage * 180f;
        viewCenter = Vector3.back;
        var angle = 45f;
        if (viewAngle > 45f) angle = Mathf.Max(90f - viewAngle, 0f);
        viewCenter = Quaternion.AngleAxis(angle, Vector3.right) * viewCenter;
    }

    private void CalculateAverageTileSize()
    {
        var tilesCount = TilesCount;
        var num = 0.0;
        var num2 = 0;
        for (var i = 0; i < tilesCount; i++)
        {
            var tileCenter = GetTileCenter(i);
            var num3 = i + 1 < tileIDToNeighbors_offsets.Count
                ? tileIDToNeighbors_offsets[i + 1]
                : tileIDToNeighbors_values.Count;
            for (var j = tileIDToNeighbors_offsets[i]; j < num3; j++)
            {
                var tileID = tileIDToNeighbors_values[j];
                var tileCenter2 = GetTileCenter(tileID);
                num += Vector3.Distance(tileCenter, tileCenter2);
                num2++;
            }
        }

        averageTileSize = (float)(num / num2);
    }

    public Vector3 GetTileCenter(int tileID)
    {
        var num = tileID + 1 < tileIDToVerts_offsets.Count ? tileIDToVerts_offsets[tileID + 1] : verts.Count;
        var a = Vector3.zero;
        var num2 = 0;
        for (var i = tileIDToVerts_offsets[tileID]; i < num; i++)
        {
            a += verts[i];
            num2++;
        }

        return a / num2;
    }

    public bool InBounds(int tileID)
    {
        return (ulong)tileID < (ulong)TilesCount;
    }

    public Vector2 LongLatOf(int tileID)
    {
        var tileCenter = GetTileCenter(tileID);
        var x = Mathf.Atan2(tileCenter.x, -tileCenter.z) * 57.29578f;
        var y = Mathf.Asin(tileCenter.y / 100f) * 57.29578f;
        return new Vector2(x, y);
    }
}