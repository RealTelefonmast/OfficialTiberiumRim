using UnityEngine;
using Verse;

namespace TR.Components;

public class MapComponent_Terrain : MapComponent
{
    public Color[] ColorOffsetGrid;

    public MapComponent_Terrain(Map map) : base(map)
    {
        ColorOffsetGrid = new Color[map.cellIndices.NumGridCells];
    }

    private void GenerateNoise()
    {
    }

    public void Set(IntVec3 c, Color value)
    {
        ColorOffsetGrid[map.cellIndices.CellToIndex(c)] = value;
    }
}

public class ColorOffsetGrid : ICellBoolGiver
{
    public Color[] colorGrid;
    public Map map;

    public ColorOffsetGrid(Map map)
    {
        this.map = map;
        colorGrid = new Color[map.cellIndices.NumGridCells];
    }

    public bool GetCellBool(int index)
    {
        return true;
    }

    public Color GetCellExtraColor(int index)
    {
        return Color.cyan;
    }

    public Color Color => Color.cyan;
}