using System.Collections.Generic;
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
        //Full Info for a terrainOptions's use
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

        public BoolGrid floraBools;
        public BoolGrid growBools;
        public CellBoolDrawer drawer;

        public TiberiumFloraGrid(Map map)
        {
            this.map = map;
            floraBools = new BoolGrid(map);
            growBools = new BoolGrid(map);
            drawer = new CellBoolDrawer(this, map.Size.x, map.Size.z, 0.35f);
        }

        //
        public void SetFlora(IntVec3 c, bool value)
        {
            floraBools.Set(c, value);
        }

        public void SetGrow(IntVec3 c, bool value)
        {
            growBools.Set(c, value);
        }

        //Bool Getters
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


        public void Notify_PlantSpawned(TiberiumPlant plant)
        {
        }
    }
}
