using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using UnityEngine;

namespace TiberiumRim
{
    public class CompTNW_TNC : CompTNW
    {
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            TNWManager.MainController = this;
        }

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

        public override Color[] ColorOverrides => new Color[] { Color.white, Network.GeneralColor, Color.white };
        public override float[] OpacityFloats => new float[] { 1f, Network.GeneralColor.a, 1f };
        public override bool[] DrawBools => new bool[] { true, true, true };

        public override string CompInspectStringExtra()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(base.CompInspectStringExtra().TrimEndNewlines());
            if (DebugSettings.godMode)
            {
                sb.AppendLine("Connected Structures: " + Network.NetworkSet.FullList.Count);

            }
            return sb.ToString().TrimEndNewlines();
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo g in base.CompGetGizmosExtra())
            {
                yield return g;
            }
        }
    }
}
