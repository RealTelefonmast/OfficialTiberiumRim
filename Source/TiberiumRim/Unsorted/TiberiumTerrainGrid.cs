using System.Collections.Generic;
using System.Linq;
using RimWorld;
using TiberiumRim;
using Verse;

namespace TR.Grids;

public class TiberiumTerrainGrid : IExposable
{
    private readonly HashSet<IntVec3> allInfestableCells = new();
    private readonly TiberiumTerrain[] terrainGrid;
    private BoolGrid boolGrid;
    private CellBoolDrawer cellBoolDrawer;
    private bool isDirty;
    private Map map;

    public TiberiumTerrainGrid()
    {
    }

    public TiberiumTerrainGrid(Map map)
    {
        boolGrid = new BoolGrid(map);
        terrainGrid = new TiberiumTerrain[map.cellIndices.NumGridCells];
    }

    public HashSet<IntVec3> AllInfestableCells
    {
        get
        {
            allInfestableCells.Clear();
            allInfestableCells.AddRange(map.AllCells.Where(CanBeInfested));
            return allInfestableCells;
        }
    }

    public void ExposeData()
    {
        Scribe_Deep.Look(ref boolGrid, "boolGrid");
    }

    public bool IsInfested(IntVec3 cell)
    {
        return boolGrid[cell];
    }

    public bool CanBeInfested(IntVec3 cell)
    {
        if (!cell.InBounds(map)) return false;
        var edifice = cell.GetEdifice(map);
        return edifice == null;
    }

    public void SetInfested(IntVec3 cell, TiberiumCrystalDef crystalDef, TerrainType type)
    {
        if (!cell.InBounds(map)) return;

        boolGrid.Set(cell, true);
        map.mapDrawer.MapMeshDirty(cell, MapMeshFlagDefOf.Terrain, true, false);
        map.fertilityGrid.Drawer.SetDirty();

        terrainGrid[cell.Index(map)] = new TiberiumTerrain(crystalDef, type);
    }

    public bool CanInfest(IntVec3 c)
    {
        return CanBeInfested(c) && !IsInfested(c);
    }
}