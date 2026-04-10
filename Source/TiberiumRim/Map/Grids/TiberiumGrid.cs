using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace TR.Grids;

/* Tiberium Grid, keeps track of all cells related to Tiberium
 * Determines growth patterns via a deferred dirty system.
 */
public class TiberiumGrid : ICellBoolGiver, IExposable
{
    // Deferred evaluation
    private readonly BoolGrid _dirtyGrid;
    private bool _hasDirty;

    // Cells adjacent to tiberium, affected by its presence
    public BoolGrid AffectedCells;
    public CellBoolDrawer Drawer;

    // Field color grids per TiberiumValueType index (Green=0, Blue=1, Red=2)
    public BoolGrid[] fieldColorGrids;

    //TODO: ForceTo was referenced in TiberiumMapInfo.CanGrowTo — needs reimplementation or removal
    public BoolGrid ForceTo;

    // Tiberium may grow from these cells
    public BoolGrid GrowFromGrid;

    // Tiberium may grow to these cells
    public BoolGrid GrowToGrid;

    public Map map;

    // Tiberium exists at these cells
    public BoolGrid TiberiumBoolGrid;

    // Per-cell crystal references
    public TiberiumCrystal[] TiberiumCrystals;

    public TiberiumGrid()
    {
    }

    public TiberiumGrid(Map map)
    {
        this.map = map;

        TiberiumBoolGrid = new BoolGrid(map);
        GrowFromGrid = new BoolGrid(map);
        GrowToGrid = new BoolGrid(map);
        AffectedCells = new BoolGrid(map);
        ForceTo = new BoolGrid(map);
        _dirtyGrid = new BoolGrid(map);

        fieldColorGrids = new[] { new BoolGrid(map), new BoolGrid(map), new BoolGrid(map) };

        Drawer = new CellBoolDrawer(this, map.Size.x, map.Size.z, 0.35f);

        TiberiumCrystals = new TiberiumCrystal[map.cellIndices.NumGridCells];
    }

    // ICellBoolGiver
    public Color Color => Color.white;

    public bool GetCellBool(int index)
    {
        return true;
    }

    public Color GetCellExtraColor(int index)
    {
        if (_dirtyGrid[index]) return Color.yellow;
        if (GrowToGrid[index]) return Color.cyan;
        if (AffectedCells[index]) return Color.magenta;
        if (GrowFromGrid[index]) return Color.green;
        return Color.clear;
    }

    public void ExposeData()
    {
    }

    // --- Crystal Registration ---

    public void SetCrystal(TiberiumCrystal crystal)
    {
        TiberiumCrystals[Index(crystal.Position)] = crystal;
        TiberiumBoolGrid.Set(crystal.Position, true);
        MarkDirty(crystal.Position);
    }

    public void ResetCrystal(IntVec3 c)
    {
        TiberiumCrystals[Index(c)] = null;
        TiberiumBoolGrid.Set(c, false);

        foreach (var adj in Adjacent(c))
            MarkDirty(adj);
    }

    // --- Bulk Reset ---

    public void ResetAll(List<TiberiumCrystal> crystals)
    {
        ClearAllGrids();
        foreach (var crystal in crystals) SetCrystal(crystal);
    }

    public void ResetAllReverse(List<TiberiumCrystal> crystals)
    {
        ClearAllGrids();
        for (var i = crystals.Count - 1; i > 0; i--) SetCrystal(crystals[i]);
    }

    private void ClearAllGrids()
    {
        TiberiumBoolGrid = new BoolGrid(map);
        GrowFromGrid = new BoolGrid(map);
        GrowToGrid = new BoolGrid(map);
        AffectedCells = new BoolGrid(map);
        fieldColorGrids = new[] { new BoolGrid(map), new BoolGrid(map), new BoolGrid(map) };
        TiberiumCrystals = new TiberiumCrystal[map.cellIndices.NumGridCells];
    }

    // --- Field Colors ---

    public void SetFieldColor(IntVec3 c, bool value, TiberiumValueType type)
    {
        switch (type)
        {
            case TiberiumValueType.Green: fieldColorGrids[0][c] = value; break;
            case TiberiumValueType.Blue: fieldColorGrids[1][c] = value; break;
            case TiberiumValueType.Red: fieldColorGrids[2][c] = value; break;
        }
    }

    // --- Deferred Evaluation ---

    public void Tick()
    {
        if (!_hasDirty) return;
        foreach (var crystal in TiberiumCrystals)
        {
            if (crystal == null) continue;
            if (_dirtyGrid[crystal.Position]) ReEvaluate(crystal.Position);
        }

        _dirtyGrid.Clear();
        _hasDirty = false;
    }

    private void MarkDirty(IntVec3 c)
    {
        _dirtyGrid.Set(c, true);
        _hasDirty = true;
    }

    public void ReEvaluate(IntVec3 c)
    {
        var adjacent = Adjacent(c);
        SetGrowFrom(c, adjacent);
        SetGrowToFor(c, adjacent, TiberiumCrystals[Index(c)]);
        SetAffected(c, adjacent);

        foreach (var adj in adjacent) Evaluate(adj);
    }

    private void Evaluate(IntVec3 c)
    {
        var adjacent = Adjacent(c);
        SetGrowFrom(c, adjacent);
        SetGrowToFor(c, adjacent, TiberiumCrystals[Index(c)]);
        SetAffected(c, adjacent);
    }

    // --- Grid Logic ---

    private void SetGrowFrom(IntVec3 c, List<IntVec3> adjacent)
    {
        GrowFromGrid[c] = TiberiumBoolGrid[c] && adjacent.Any(a => !TiberiumBoolGrid[a] && !a.HasTibFlora(map));
    }

    private void SetAffected(IntVec3 c, List<IntVec3> adjacent)
    {
        AffectedCells[c] = !TiberiumBoolGrid[c] && adjacent.Any(v => TiberiumBoolGrid[v]);
    }

    private void SetGrowToFor(IntVec3 c, List<IntVec3> adjacent, TiberiumCrystal crystal)
    {
        var cells = AdjacentGrowToCells(c, out var nearTib);
        if (crystal == null)
        {
            if (!nearTib) GrowToGrid[c] = false;
            return;
        }

        if (GrowFromGrid[c]) GrowToGrid[c] = false;
        if (cells.NullOrEmpty()) return;
        var cell = cells[Rand.RangeSeeded(0, cells.Count - 1, crystal.GetHashCode())];
        GrowToGrid[cell] = true;
    }

    private List<IntVec3> AdjacentGrowToCells(IntVec3 origin, out bool nearTib)
    {
        var cells = new List<IntVec3>();
        nearTib = false;
        for (var i = 0; i < 8; i++)
        {
            var cell = origin + GenAdj.AdjacentCells[i];
            if (!cell.InBounds(map)) continue;
            if (TiberiumBoolGrid[cell])
            {
                nearTib = true;
                continue;
            }

            if (cell.HasTibFlora(map)) continue;
            cells.Add(cell);
        }

        return cells;
    }

    // --- Helpers ---

    private int Index(IntVec3 c)
    {
        return map.cellIndices.CellToIndex(c);
    }

    private List<IntVec3> Adjacent(IntVec3 origin, bool andInside = false, Predicate<IntVec3> predicate = null)
    {
        var count = andInside ? 9 : 8;
        var cells = new List<IntVec3>();
        for (var i = 0; i < count; i++)
        {
            var cell = origin + GenAdj.AdjacentCellsAndInside[i];
            if (cell.InBounds(map) && (predicate == null || predicate(cell)))
                cells.Add(cell);
        }

        return cells;
    }
}