using System.Collections.Generic;
using System.Text;
using TR.Defs;
using TR.GameParts.Networks.TiberiumNetwork;
using TR.TiberiumObjects;
using UnityEngine;
using UnityEngine.UIElements;
using Verse;

namespace TR.TiberiumProcessing;

public class TNW_TiberiumSpike : FXBuilding
{
    public TiberiumGeyser boundGeyser;

    public Comp_TiberiumNetworkStructure CompTNW => this.TryGetComp<Comp_TiberiumNetworkStructure>();

    public override float[] OpacityFloats => new[] { 1f, 1f };

    public override bool[] DrawBools => new[]
        { CompTNW.HasConnection, CompTNW.HasConnection && CompTNW.CompPower.PowerOn };

    public override Color[] ColorOverrides => new[] { Color.white, Color.white };

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
        sb.AppendLine("\n" + "TR_GeyserContent" + ": " + boundGeyser.ContentPercent.ToStringPercent());
        return sb.ToString().TrimEndNewlines();
    }
}