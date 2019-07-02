using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;

namespace TiberiumRim
{
    public class MapComponent_UndergroundTiberium : MapComponent
    {
        public ushort[] gasGrid;
        public ushort[] crystalGrid;

        public MapComponent_UndergroundTiberium(Map map) : base(map)
        {
            this.gasGrid = new ushort[map.cellIndices.NumGridCells];
        }

        public void CreateGasDepot()
        {

        }
    }
}
