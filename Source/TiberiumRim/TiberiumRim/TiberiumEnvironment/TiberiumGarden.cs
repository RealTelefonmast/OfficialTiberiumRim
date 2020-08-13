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
        private CellArea cells;


        public TiberiumGarden(TiberiumFloraGrid floraGrid)
        {
            this.floraGrid = floraGrid;
            this.map = floraGrid.map;
            cells = new CellArea(map);
        }

        public void GardenTick()
        {

        }

        public void AddCell(IntVec3 cell)
        {
            cells.Add(cell);
        }

    }
}
