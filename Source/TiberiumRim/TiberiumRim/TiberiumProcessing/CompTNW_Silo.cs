using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace TiberiumRim
{
    public class CompTNW_Silo : CompTNW
    {
        public override bool ShouldDoEffecters => Container.StoredPercent > 0.5f;

        public override IEnumerable<IntVec3> InnerConnectionCells
        {
            get
            {
                var rect = parent.OccupiedRect();
                var cells = rect.Cells.ToList();
                rect.Corners.ToList().ForEach(x => cells.Remove(x));
                return cells;
            }
        }
    }
}
