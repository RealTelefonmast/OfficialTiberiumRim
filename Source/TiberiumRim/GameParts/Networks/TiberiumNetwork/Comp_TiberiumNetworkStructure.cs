using System.Collections.Generic;
using System.Text;
using TeleCore;
using TeleCore.Data.Events;
using TeleCore.FlowCore;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class Comp_TiberiumNetworkStructure : Comp_Network
    {
        public NetworkSubPart TiberiumNetPart => this[TiberiumDefOf.TiberiumNetwork];
        public NetworkSubPart WasteNetPart => this[TiberiumDefOf.WasteNetwork];
        
        public bool HasConnection => TiberiumNetPart.HasConnection;
        public bool HasWasteConnection => this.HasPartFor(TiberiumDefOf.WasteNetwork);
        
        
        public CompProperties_TNS TNSProps => (CompProperties_TNS)base.Props;

        public NetworkContainer Container => TiberiumNetPart.Container;
        
        public Color Color
        {
            get
            {
                if (TiberiumNetPart.Container != null)
                {
                    return TiberiumNetPart.Container.Color;
                }
                return Color.magenta;
            }
        }

        //FX
        public override Color? FX_GetColor(FXLayerArgs args)
        {
            return args.index switch
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
    public class CompProperties_TNS : CompProperties_Network
    {
        public CompProperties_TNS()
        {
            this.compClass = typeof(Comp_TiberiumNetworkStructure);
        }
    }
}
