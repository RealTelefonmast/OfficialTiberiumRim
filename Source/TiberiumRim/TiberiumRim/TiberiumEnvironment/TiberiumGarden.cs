using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class TiberiumGarden
    {
        private TiberiumFloraGrid floraGrid;
        private Map map;
        private IntVec3 center;
        private List<IntVec3> cells = new List<IntVec3>();


        public TiberiumGarden(TiberiumFloraGrid floraGrid)
        {
            this.floraGrid = floraGrid;
            this.map = floraGrid.map;
        }

        public void GardenTick()
        {

        }

        private void CalculateCenter()
        {

        }

        public void AddCell(IntVec3 cell)
        {

        }

    }
}
