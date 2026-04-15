using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class TNW_TiberiumSpike : TiberiumNetworkBuilding
    {
        public TiberiumGeyser boundGeyser;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            boundGeyser = this.Position.GetFirstThing(map, TiberiumDefOf.TiberiumGeyser) as TiberiumGeyser;
            boundGeyser.tiberiumSpike = this;
        }

        public override float[] OpacityFloats => new float[] { 1f, 1f, 1f };
        public override bool[] DrawBools => new bool[] { base.DrawBools[1], base.DrawBools[1] && CompTNW.compPower.PowerOn, true };
        public override Color[] ColorOverrides => new Color[] { Color.white, Color.white, Color.white };

        public override IEnumerable<InspectTabBase> GetInspectTabs()
        {
            return base.GetInspectTabs();
        }

        public override string GetInspectString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(base.GetInspectString());
            sb.AppendLine("TR_GeyserContent" + ": " + (boundGeyser.depositValue / boundGeyser.maxDepositValue).ToStringPercent());
            return sb.ToString().TrimEndNewlines();
        }
    }
}
