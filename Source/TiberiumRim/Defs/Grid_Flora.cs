using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace TiberiumRim;

public class TerrainDataDef : Def
{
    public List<TerrainData> terrain;
}

public class TerrainData
{
    public List<TiberiumCrystalDef> supportedCrystals;

    public bool supportsFlora = false;

    //Full Info for a terrain's use
    public TerrainDef terrain;
}

/*  The Tiberium Flora Grid keeps track of all cells that are eligibale and meant for Tiberium plant life,
 *  This is used for a more organic look of the map once it gets covered with Tiberium
 */

public class TiberiumFloraGrid : ICellBoolGiver
{
    public CellBoolDrawer drawer;
    public TiberiumFloraManager floraManager;
    public BoolGrid growBools;
    public Map map;

    public TiberiumFloraGrid(Map map)
    {
        this.map = map;
        floraManager = new TiberiumFloraManager(map);
        growBools = new BoolGrid(map);
        drawer = new CellBoolDrawer(this, map.Size.x, map.Size.z, 0.35f);
        Init();
    }

    public bool GetCellBool(int index)
    {
        return growBools[index];
    }

    public Color Color => Color.white;

    public Color GetCellExtraColor(int index)
    {
        if (growBools[index]) return Color.green;
        return Color.red;
    }

    public void Init()
    {
        LongEventHandler.QueueLongEvent(delegate
        {
            var filler = map.floodFiller;
            foreach (var cell in map.AllCells)
            {
                if (growBools[cell]) continue;
                var terrain = cell.GetTerrain(map);
                if (IsGarden(terrain))
                {
                    var garden = new TiberiumGarden(this);
                    filler.FloodFill(cell, c => c.GetTerrain(map) == terrain, delegate(IntVec3 cell)
                    {
                        Set(cell, true);
                        garden.AddCell(cell);
                    });
                }
            }
        }, "SettingFloraBools", false, null);
    }

    private bool IsGarden(TerrainDef def)
    {
        return def.IsMoss() || (def.IsSoil() && def.fertility >= 1.2f);
    }

    private bool IsPond(TerrainDef def)
    {
        return def.IsWater && !def.IsRiver;
    }

    public void Set(IntVec3 c, bool value)
    {
        growBools.Set(c, true);
    }

    public void Notify_PlantSpawned(TiberiumPlant plant)
    {
        floraManager.Notify_PlantSpawnedFromOutside(plant);
    }
}