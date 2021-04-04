using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class TiberiumTerrainInfo : MapInformation
    {
        public TiberiumWaterInfo WaterInfo;

        public TiberiumTerrainInfo(Map map) : base(map)
        {
            WaterInfo = new TiberiumWaterInfo(map);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref WaterInfo, "WaterInfo");
        }

        public override void InfoInit(bool initAfterReload = false)
        {
            base.InfoInit(initAfterReload);
            WaterInfo.InfoInit(initAfterReload);
        }

        public override void Tick()
        {
            WaterInfo.Tick();
        }

        public override void Draw()
        {
            WaterInfo.Draw();
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
