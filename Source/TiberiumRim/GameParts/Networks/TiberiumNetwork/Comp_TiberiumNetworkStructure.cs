using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace TiberiumRim
{
    public class Comp_TiberiumNetworkStructure : Comp_NetworkStructure
    {
        public NetworkComponent TiberiumComp => this[TiberiumDefOf.TiberiumNetwork];
        public NetworkContainer Container => TiberiumComp.Container;
        public bool HasConnection => TiberiumComp.HasConnection;

        public override void PostExposeData()
        {
            base.PostExposeData();
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (!respawningAfterLoad)
            {

            }
        }

        public override void PostDeSpawn(Map map)
        {
            base.PostDeSpawn(map);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(base.ToString());

            if (DebugSettings.godMode)
            {
                sb.AppendLine("Storage Mode: " + Container.AcceptedTypes.ToStringSafeEnumerable());
            }

            return sb.ToString();
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo g in Container.GetGizmos())
            {
                yield return g;
            }

            foreach (Gizmo g in base.CompGetGizmosExtra())
            {
                yield return g;
            }
        }
    }
}
