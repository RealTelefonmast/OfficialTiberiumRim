using System;
using System.Collections.Generic;
using System.Linq;
using TeleCore;
using UnityEngine;
using Verse;

namespace TiberiumRim;

public enum TerrainType
{
    Soily,
    Sandy,
    Stoney,
    Woody,
    Metallic
}

public struct TiberiumTerrain
{
    public int crystalDefID;
    public TerrainType type;

    public TiberiumTerrain(TiberiumCrystalDef crystalDef, TerrainType terrainType)
    {
        crystalDefID = crystalDef.IDReference;
        type = terrainType;
    }
}

public class TiberiumTerrainGrid : IExposable
{
    private Map map;
    private bool isDirty;
    private CellBoolDrawer cellBoolDrawer;
    private BoolGrid boolGrid;
    private TiberiumTerrain[] terrainGrid;
    
    private readonly HashSet<IntVec3> allInfestableCells = new HashSet<IntVec3>();

    public TiberiumTerrainGrid(){}
    
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
    
    public bool IsInfested(IntVec3 cell)
    {
        return boolGrid[cell];
    }
    
    public bool CanBeInfested(IntVec3 cell)
    {
        if (!cell.InBounds(map)) return false;
        Building edifice = cell.GetEdifice(map);
        return edifice == null;
    }
    
    public void SetInfested(IntVec3 cell, TiberiumCrystalDef crystalDef, TerrainType type)
    {
        if (!cell.InBounds(map)) return;
        
        boolGrid.Set(cell, true);
        map.mapDrawer.MapMeshDirty(cell, MapMeshFlag.Terrain, true, false);
        map.fertilityGrid.Drawer.SetDirty();

        terrainGrid[cell.Index(map)] = new TiberiumTerrain(crystalDef, type);
    }
    
    public bool CanInfest(IntVec3 c)
    {
        return CanBeInfested(c) && !IsInfested(c);
    }
    
    public void ExposeData()
    {
        Scribe_Deep.Look(ref boolGrid, "boolGrid", Array.Empty<object>());
    }
    
}