using System.Collections.Generic;
using System.Text;
using RimWorld;
using TeleCore.Generics.Container;
using TeleCore.Network;
using TeleCore.Types;
using UnityEngine;
using Verse;

namespace TeleCore;

public class CompPowerPlant_Network : CompPowerPlant
{
    private CompNetwork _comp;
    private INetworkPart _netPart;
    private float internalPowerOutput;
    private int powerTicksRemaining;

    public new CompProperties_NetworkPowerPlant Props => (CompProperties_NetworkPowerPlant)base.Props;

    public bool GeneratesPowerNow => powerTicksRemaining > 0;
    public override float DesiredPowerOutput => internalPowerOutput;
    public bool IsAtCapacity => powerTicksRemaining >= Props.maxWorkTime.TotalTicks;

    public bool CanConsume => !IsAtCapacity; //TODO: && !_netPart.RequestWorker.RequestingNow;

    public override void PostExposeData()
    {
        base.PostExposeData();
        //Scribe_Values.Look(ref powerProductionTicks, "powerTicks");
    }

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);
        _comp = parent.GetComp<CompNetwork>();
        _netPart = _comp[Props.fromNetwork];
    }

    public override void CompTick()
    {
        base.CompTick();
        PowerTick();
    }

    private void PowerTick()
    {
        base.CompTick();
        //If no power-generation possible
        if (!PowerOn || Props.valueToTickRules.NullOrEmpty())
        {
            internalPowerOutput = 0f;
            return;
        }

        //If no value
        if (CanConsume && _netPart.Volume.FillState != ContainerFillState.Empty)
            foreach (var conversion in Props.valueToTickRules)
            {
                if (_netPart.Volume.StoredValueOf(conversion.valueDef) <= 0) continue;
                if (_netPart.Volume.TryConsume(conversion.valueDef, conversion.cost))
                    powerTicksRemaining += Mathf.RoundToInt(conversion.seconds.SecondsToTicks());
            }

        if (powerTicksRemaining > 0)
        {
            internalPowerOutput = -base.Props.basePowerConsumption;
            powerTicksRemaining--;
        }
    }

    public override IEnumerable<Gizmo> CompGetGizmosExtra()
    {
        foreach (var gizmo in base.CompGetGizmosExtra()) yield return gizmo;

        yield return new Command_Action
        {
            defaultLabel = "Clear Power",
            defaultDesc = "Clears the power buffer",
            action = () => powerTicksRemaining = 0
        };
    }

    public override string CompInspectStringExtra()
    {
        var sb = new StringBuilder();
        sb.AppendLine(base.CompInspectStringExtra());
        if (GeneratesPowerNow)
            sb.AppendLine("TR_PowerLeft".Translate(powerTicksRemaining.ToStringTicksToPeriod()));
        return sb.ToString().TrimEndNewlines();
    }
}

public class CompProperties_NetworkPowerPlant : CompProperties_Power
{
    public NetworkDef fromNetwork;
    public TickTime maxWorkTime = new(GenDate.TicksPerDay);
    public List<ValueConversion> valueToTickRules;
}

public class ValueConversion
{
    public float cost;
    public float seconds;
    public NetworkValueDef valueDef;
}