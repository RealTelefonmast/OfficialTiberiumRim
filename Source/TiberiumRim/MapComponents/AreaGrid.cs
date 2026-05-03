using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace TR.Grids;

public class AreaGrid : IExposable, ICellBoolGiver
{
    private readonly Map map;
    private CellBoolDrawer drawer;
    private BoolGrid grid;

    public AreaGrid()
    {
    }

    public AreaGrid(Map map)
    {
        this.map = map;
        grid = new BoolGrid(map);
    }

    public IEnumerable<IntVec3> Cells => grid.ActiveCells;

    public int CellCount => grid.TrueCount;

    private CellBoolDrawer Drawer => drawer ??= new CellBoolDrawer(this, map.Size.x, map.Size.z);

    public bool this[int index]
    {
        get => grid[index];
        set => Set(map.cellIndices.IndexToCell(index), value);
    }

    public bool this[IntVec3 c]
    {
        get => grid[map.cellIndices.CellToIndex(c)];
        set => Set(c, value);
    }

    public bool GetCellBool(int index)
    {
        return grid[index];
    }

    public Color GetCellExtraColor(int index)
    {
        return Color;
    }

    public Color Color => Color.cyan;

    public void ExposeData()
    {
        Scribe_Deep.Look(ref grid, "innerGrid");
    }

    public virtual void Set(IntVec3 c, bool val)
    {
        var index = map.cellIndices.CellToIndex(c);
        if (grid[index] == val)
            return;

        grid[index] = val;
        MarkDirty(c);
    }

    private void MarkDirty(IntVec3 c)
    {
        Drawer.SetDirty();
    }

    public void MarkForDraw()
    {
        if (map == Find.CurrentMap) Drawer.MarkForDraw();
    }

    public void AreaUpdate()
    {
        Drawer.CellBoolDrawerUpdate();
    }
}