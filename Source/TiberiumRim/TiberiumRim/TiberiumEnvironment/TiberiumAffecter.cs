using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace TiberiumRim
{
    public class TiberiumAffecter : MapInformation
    {
        private BoolGrid affectedCells;

        public TiberiumHediffGrid hediffGrid;

        public TiberiumAffecter(Map map) : base(map)
        {
            affectedCells = new BoolGrid(map);
            hediffGrid = new TiberiumHediffGrid(map);
        }

        public override void ExposeData()
        {
            Scribe_Deep.Look(ref affectedCells, "affectedCells");
            Scribe_Deep.Look(ref hediffGrid, "hediffGrid");
        }
    }
}
