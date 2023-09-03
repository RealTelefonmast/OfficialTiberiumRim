using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using TeleCore;
using TeleCore.Static;
using UnityEngine;
using Verse;

namespace TR;

/*
 * Tiberium Grid, keeps track of all cells related to Tiberium
 * Determines growth patterns
 */

[Serializable]
public struct TiberiumCellState
{
    public bool Infested { get; set; }
}

public class TiberiumGrid2 : IExposable
{
    private List<IntVec3> infestedCellsThisTick = new List<IntVec3>();
    private TiberiumCellState[] grid;
    private Map map;
    private bool dirty;

    public List<IntVec3> AllOpenCells
    {
        get
        {
            var list = StaticListHolder<IntVec3>.RequestList($"TiberiumInfestedCells_{map.uniqueID}");
            list.AddRange(map.AllCells.Where(CanBeTiberiumInfested));
            return list;
        }
    }

    public TiberiumGrid2(Map map)
    {
        this.map = map;
        grid = new TiberiumCellState[map.cellIndices.NumGridCells];
    }
    
    public void ExposeData()
    {
        
    }

    public bool IsInfested(IntVec3 cell)
    {
        return cell.InBounds(map) && grid[cell.Index(map)].Infested;
    }
    
    public bool CanBeTiberiumInfested(IntVec3 cell)
    {
        if (!cell.InBounds(map)) return false;
        return true;
    }
}

public class TiberiumGrid : ICellBoolGiver
{
    private readonly Map map;

    //Crystals
    public readonly BoolGrid BoolGrid;
    public readonly TiberiumCrystal[] TiberiumCrystals;

    //
    public readonly BoolGrid GrowToGrid;
    public readonly BoolGrid GrowFromGrid;

    public readonly BoolGrid AffectedCells;

    public readonly CellBoolDrawer Drawer;

    public Color Color => Color.white;

    public TiberiumGrid()
    {
    }

    public TiberiumGrid(Map map)
    {
        this.map = map;
        BoolGrid = new BoolGrid(map);
        GrowToGrid = new BoolGrid(map);
        GrowFromGrid = new BoolGrid(map);
        AffectedCells = new BoolGrid(map);

        Drawer = new CellBoolDrawer(this, map.Size.x, map.Size.z, 0.35f);

        TiberiumCrystals = new TiberiumCrystal[map.cellIndices.NumGridCells];
    }

    public void ExposeData()
    {
    }

    public bool GetCellBool(int index)
    {
        return true;
    }

    public Color GetCellExtraColor(int index)
    {
        Color color = Color.clear;
        if (GrowToGrid[index])
        {
            color = Color.cyan;
            return color;
        }

        if (AffectedCells[index])
        {
            color = Color.magenta;
            return color;
        }

        if (GrowFromGrid[index])
        {
            color = Color.green;
            return color;
        }

        return color;
    }

    private int Index(IntVec3 c)
    {
        return map.cellIndices.CellToIndex(c);
    }

    //
    private List<IntVec3> Adjacent(IntVec3 origin, bool andInside = false, Predicate<IntVec3> predicate = null)
    {
        int count = andInside ? 9 : 8;
        List<IntVec3> cells = new List<IntVec3>();
        for (int i = 0; i < count; i++)
        {
            var cell = origin + GenAdj.AdjacentCellsAndInside[i];
            if (cell.InBounds(map) && (predicate == null || predicate(cell)))
                cells.Add(cell);
        }

        return cells;
    }

    //
    public void SetCrystal(TiberiumCrystal crystal)
    {
        Rand.PushState(crystal.GetHashCode());

        BoolGrid[crystal.Position] = true;
        TiberiumCrystals[Index(crystal.Position)] = crystal;

        var adjacent = Adjacent(crystal.Position, true);
        foreach (var adj in adjacent)
        {
            var adjacent2 = Adjacent(adj);
            SetGrowFrom(adj, adjacent2);
            SetAffected(adj, adjacent2);
        }

        SetGrowTo(crystal.Position, crystal);

        Rand.PopState();
    }

    public void ResetCrystal(IntVec3 c)
    {
        BoolGrid[c] = false;
        TiberiumCrystals[Index(c)] = null;
        GrowFromGrid[c] = false;
        RemoveGrowTo(c, Adjacent(c));

        var adjacent = Adjacent(c);
        foreach (var adj in adjacent)
        {
            var adjacent2 = Adjacent(adj);
            SetGrowFrom(adj, adjacent2);
            SetAffected(adj, adjacent2);
            UpdateGrowTo(adj, adjacent2);
        }
    }

    private void SetGrowFrom(IntVec3 c, List<IntVec3> adjacent)
    {
        GrowFromGrid[c] = BoolGrid[c] && adjacent.Any(a => !BoolGrid[a] && !a.HasTibFlora(map));
    }

    private void SetAffected(IntVec3 c, List<IntVec3> adjacent)
    {
        AffectedCells[c] = !BoolGrid[c] && adjacent.Any(v => BoolGrid[v]);
    }

    private void RemoveGrowTo(IntVec3 c, List<IntVec3> adjacent)
    {
        //-> Removal
        GrowToGrid[c] = adjacent.Any(c => BoolGrid[c]);
        if (adjacent.Any(c => BoolGrid[c]))
        {
            GrowToGrid[c] = true;
            return;
        }
    }

    private void SetGrowTo(IntVec3 c, TiberiumCrystal crystal)
    {
        //-> Addition
        if (crystal == null) return;
        if (BoolGrid[c])
        {
            GrowToGrid[c] = false;
        }

        var cells = Adjacent(c, false, x => !BoolGrid[x] && !x.HasTibFlora(map));
        if (cells.NullOrEmpty()) return;
        if (Rand.Chance(crystal.def.tiberium.rootNodeChance))
        {
            cells.Do(x => GrowToGrid[x] = true);
            return;
        }

        var cell = cells[Rand.Range(0, cells.Count - 1)];
        GrowToGrid[cell] = true;
    }

    private void UpdateGrowTo(IntVec3 c, List<IntVec3> adjacent)
    {
        var crystal = TiberiumCrystals[Index(c)];
        var hasNeighbor = adjacent.Any(c => BoolGrid[c]);
        if (GrowToGrid[c] && hasNeighbor)
        {
            return;
        }

        if (crystal != null)
        {
            SetGrowTo(c, crystal);
            return;
        }

        GrowToGrid[c] = false;
    }
}
