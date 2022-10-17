using TeleCore;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class TiberiumTerrainInfo : MapInformation
    {
        private TiberiumWaterInfo waterInfo;

        public TiberiumWaterInfo WaterInfo => waterInfo;
        
        public TiberiumTerrainInfo(Map map) : base(map)
        {
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref waterInfo, "WaterInfo", map);
        }

        public override void InfoInit(bool initAfterReload = false)
        {
            base.InfoInit(initAfterReload);
            waterInfo = map.GetMapInfo<TiberiumWaterInfo>();
            waterInfo.InfoInit(initAfterReload);
        }

        public override void Tick()
        {
            waterInfo.Tick();
        }

        public override void Update()
        {
            waterInfo.Update();
        }

        public void Notify_TibSpawned(TiberiumCrystal crystal)
        {
            waterInfo.Notify_TibSpawned(crystal);
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
