using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class TerrainDataDef : Def
    {
        public List<TerrainData> terrain;
    }

    public class TerrainData
    {
        //Full Info for a terrain's use
        public TerrainDef terrain;
        public bool supportsFlora = false;

        public List<TiberiumCrystalDef> supportedCrystals;
    }

    /*  The Tiberium Flora Grid keeps track of all cells that are eligibale and meant for Tiberium plant life, 
     *  This is used for a more organic look of the map once it gets covered with Tiberium
     */ 

    public class TiberiumFloraGrid : ICellBoolGiver
    {
        public Map map;
        public TiberiumFloraManager floraManager;
        public BoolGrid growBools;
        public CellBoolDrawer drawer;

        public TiberiumFloraGrid(Map map)
        {
            this.map = map;
            floraManager = new TiberiumFloraManager(map);
            growBools = new BoolGrid(map);
            drawer = new CellBoolDrawer(this, map.Size.x, map.Size.z, 0.35f);
            Init();
        }

        public void Init()
        {
            LongEventHandler.QueueLongEvent(delegate ()
            {
                FloodFiller filler = map.floodFiller;
                foreach (IntVec3 cell in map.AllCells)
                {
                    if (growBools[cell]) continue;
                    TerrainDef terrain = cell.GetTerrain(map);
                    if(NeedsFlora(terrain))
                    {
                        TiberiumGarden garden = new TiberiumGarden(map.areaManager);
                        filler.FloodFill(cell, ((IntVec3 c) => c.GetTerrain(map) == terrain), delegate (IntVec3 cell) {
                            Set(cell, true);
                            garden[cell] = true;
                        });
                    }
                }
            }, "SettingFloraBools", false, null);
        }

        private bool NeedsFlora(TerrainDef def)
        {
            return def.IsMoss() || (def.IsSoil() && (def.fertility >= 1.2f));
        }

        public bool GetCellBool(int index)
        {
            return growBools[index];
        }

        public Color Color => Color.white;

        public Color GetCellExtraColor(int index)
        {
            if (growBools[index])
            {
                return Color.green;
            }
            return Color.red;
        }

        public void Set(IntVec3 c, bool value)
        {
            growBools.Set(c, true);
        }
    }
}
