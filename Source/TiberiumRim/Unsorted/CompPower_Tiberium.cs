using System.Text;
using RimWorld;
using TR.Networks.TiberiumNetwork;
using Verse;

namespace TR;

public class CompPower_Tiberium : CompPowerPlant
{
    private CompTNS_Power _powerNetComp;
    private int powerProductionTicks;

    private CompProperties_TNSPower TNSProps => (CompProperties_TNSPower)_powerNetComp.props;
    public bool GeneratesPowerNow => powerProductionTicks > 0;

    protected override float DesiredPowerOutput => GeneratesPowerNow ? base.DesiredPowerOutput : 0f;

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Values.Look(ref powerProductionTicks, "powerTicks");
    }

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);
        _powerNetComp = parent.GetComp<CompTNS_Power>();
    }

    public override void CompTick()
    {
        base.CompTick();
        PowerTick();
    }

    private void PowerTick()
    {
        if (powerProductionTicks <= 0)
        {
            var container = _powerNetComp?.Container;
            if (container != null && container.TryConsumeOrFail(container.MainValueDef, TNSProps.consumeAmt))
                powerProductionTicks = (int)(GenDate.TicksPerDay * TNSProps.daysPerLoad);
        }
        else
        {
            powerProductionTicks--;
        }
    }

    public override string CompInspectStringExtra()
    {
        var sb = new StringBuilder();
        sb.AppendLine(base.CompInspectStringExtra());
        if (GeneratesPowerNow)
            sb.AppendLine("TR_PowerLeft".Translate(powerProductionTicks.ToStringTicksToPeriod()));
        return sb.ToString().TrimEndNewlines();
    }
}