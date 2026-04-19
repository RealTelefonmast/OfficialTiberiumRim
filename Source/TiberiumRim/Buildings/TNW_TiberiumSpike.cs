using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace TiberiumRim;

public class TNW_TiberiumSpike : TiberiumNetworkBuilding
{
    public TiberiumGeyser boundGeyser;

    public override float[] OpacityFloats => new[] { 1f, 1f, 1f };

    public override bool[] DrawBools =>
        new[] { base.DrawBools[1], base.DrawBools[1] && CompTNW.compPower.PowerOn, true };

    public override Color[] ColorOverrides => new[] { Color.white, Color.white, Color.white };

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
        boundGeyser = Position.GetFirstThing(map, TiberiumDefOf.TiberiumGeyser) as TiberiumGeyser;
        boundGeyser.tiberiumSpike = this;
    }

    public override IEnumerable<InspectTabBase> GetInspectTabs()
    {
        return base.GetInspectTabs();
    }

    public override string GetInspectString()
    {
        var sb = new StringBuilder();
        sb.AppendFormat(base.GetInspectString());
        sb.AppendLine("TR_GeyserContent" + ": " +
                      (boundGeyser.depositValue / boundGeyser.maxDepositValue).ToStringPercent());
        return sb.ToString().TrimEndNewlines();
    }
}