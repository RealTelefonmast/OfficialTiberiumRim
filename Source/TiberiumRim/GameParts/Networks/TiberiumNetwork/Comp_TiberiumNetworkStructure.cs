using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeleCore;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class Comp_TiberiumNetworkStructure : Comp_NetworkStructure
    {
        public NetworkSubPart TiberiumComp => this[TiberiumDefOf.TiberiumNetwork];
        public NetworkContainer Container => TiberiumComp.Container;
        public bool HasConnection => TiberiumComp.HasConnection;

        public new CompProperties_TNS Props => (CompProperties_TNS)base.Props;

        public Color Color
        {
            get
            {
                if (TiberiumComp.Container != null)
                {
                    return TiberiumComp.Container.Color;
                }
                return Color.magenta;
            }
        }

        //FX
        public override Color? FX_GetColorAt(int index)
        {
            return index switch
            {
                0 => Color,
                _ => Color.white
            };
        }

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
            foreach (Gizmo g in base.CompGetGizmosExtra())
            {
                yield return g;
            }
        }
    }
    public class CompProperties_TNS : CompProperties_NetworkStructure
    {
        public CompProperties_TNS()
        {
            this.compClass = typeof(Comp_TiberiumNetworkStructure);
        }
    }
}
