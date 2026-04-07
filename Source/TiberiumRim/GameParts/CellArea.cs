using System.Collections.Generic;
using Verse;

namespace TR.GameParts;

public class CellArea : IExposable
{
    private bool[] cellBools;
    private int mapSizeX;
    private int mapSizeZ;
    private int trueCountInt;

    private bool withBorder;

    public CellArea()
    {
    }

    public CellArea(Map map, bool withBorder = false)
    {
        Cells = new List<IntVec3>();
        mapSizeX = map.Size.x;
        mapSizeZ = map.Size.z;
        cellBools = new bool[mapSizeZ * mapSizeX];
        trueCountInt = 0;
        this.withBorder = withBorder;
    }

    public List<IntVec3> Cells { get; } = new();

    public List<IntVec3> Border { get; } = new();

    public int Count => Cells.Count;

    public IntVec3 this[int i]
    {
        get => Cells[i];
        set => Cells[i] = value;
    }

    public void ExposeData()
    {
        Scribe_Values.Look(ref trueCountInt, "trueCount");
        Scribe_Values.Look(ref mapSizeX, "mapSizeX");
        Scribe_Values.Look(ref mapSizeZ, "mapSizeZ");

        DataExposeUtility.LookBoolArray(ref cellBools, mapSizeZ * mapSizeX, "cellBools");

        if (Scribe.mode == LoadSaveMode.PostLoadInit)
            for (var index = 0; index < cellBools.Length; index++)
            {
                var cell = cellBools[index];
                if (cell)
                    Cells.Add(CellIndicesUtility.IndexToCell(index, mapSizeX));
            }
    }

    public void Add(IntVec3 cell)
    {
        Cells.Add(cell);
        cellBools[CellIndicesUtility.CellToIndex(cell, mapSizeX)] = true;
        trueCountInt++;
    }

    public void AddRange(List<IntVec3> newCells)
    {
        foreach (var cell in newCells)
        {
            Cells.Add(cell);
            cellBools[CellIndicesUtility.CellToIndex(cell, mapSizeX)] = true;
            trueCountInt++;
        }
    }

    public void Remove(IntVec3 cell)
    {
        if (Cells.Remove(cell))
        {
            cellBools[CellIndicesUtility.CellToIndex(cell, mapSizeX)] = false;
            trueCountInt--;
        }
    }

    public bool Contains(IntVec3 cell)
    {
        return Cells.Contains(cell);
    }

    public bool Empty()
    {
        return Cells.NullOrEmpty();
    }
}