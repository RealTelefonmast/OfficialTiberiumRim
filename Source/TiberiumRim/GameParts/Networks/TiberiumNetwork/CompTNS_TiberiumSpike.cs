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
        public override float[] OpacityFloats => new float[] { 1f, 1f };
        public override bool[] DrawBools => new bool[] { HasConnection, HasConnection && CompPower.PowerOn };
        public override Color[] ColorOverrides => new Color[] { Color.white, Color.white };

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
