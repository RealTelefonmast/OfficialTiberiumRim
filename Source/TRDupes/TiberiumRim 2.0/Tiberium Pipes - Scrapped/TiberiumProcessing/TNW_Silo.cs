using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace TiberiumRim
{
    public class TNW_Silo : TiberiumNetworkBuilding
    {
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            base.DeSpawn(mode);
        }

        public override IEnumerable<IntVec3> ConnectableCells
        {
            get
            {
                var rect = this.OccupiedRect();
                var cells = rect.Cells.ToList();
                rect.Corners.ToList().ForEach(x => cells.Remove(x));
                return cells;
            }
        }
    }
}
