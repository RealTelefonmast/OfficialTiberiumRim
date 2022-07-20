using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class CompTNS_TiberiumSpike : Comp_TiberiumNetworkStructure
    {
        //FX
        public override bool FX_ShouldDrawAt(int index)
        {
            return index switch
            {
                0 => HasConnection,
                1 => HasConnection && CompPower.PowerOn,
                _ => base.FX_ShouldDrawAt(index)
            };
        }

        public override Color? FX_GetColorAt(int index)
        {
            return index switch
            {
                _ => Color.white
            };
        }

        public override float FX_GetOpacityAt(int index)
        {
            return index switch
            {
                _ => 1f
            };
        }

        //
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
        }

        public override void PostDeSpawn(Map map)
        {
            base.PostDeSpawn(map);
        }

        public override string CompInspectStringExtra()
        {
            //StringBuilder sb = new StringBuilder();
            //sb.AppendFormat(base.CompInspectStringExtra());
            //sb.AppendLine("\n" + "TR_GeyserContent" + ": " + boundGeyser.ContentPercent.ToStringPercent());
            return base.CompInspectStringExtra();  //sb.ToString().TrimEndNewlines();
        }
    }
}
