using TeleCore;
using UnityEngine;
using Verse;

namespace TR
{
    public class TRTerrainInfo : MapInformation
    {
        //
        private TiberiumTerrainGrid terrainGrid;
        private TRWaterInfo waterInfo;
        
        //
        public TRWaterInfo WaterInfo
        {
            get
            {
                return waterInfo ??= map.GetMapInfo<TRWaterInfo>();
            }
        }

        public TRTerrainInfo(Map map) : base(map)
        {
            terrainGrid = new TiberiumTerrainGrid(map);
        }

        public override void ExposeDataExtra()
        {
            Scribe_Deep.Look(ref terrainGrid, "terrainGrid");
        }

        public override void InfoInit(bool initAfterReload = false)
        {
            base.InfoInit(initAfterReload);
        }

        public override void Tick()
        {
            WaterInfo.Tick();
        }

        public override void Update()
        {
            WaterInfo.Update();
        }

        public void Notify_TibSpawned(TiberiumCrystal crystal)
        {
            WaterInfo.Notify_TibSpawned(crystal);
        }

    }

    public class ColorOffsetGrid : ICellBoolGiver
    {
        public Map map;
        public Color[] colorGrid;

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

        public Color Color
        {
            get
            {
                return Color.cyan;
            }
        }
    }
}
