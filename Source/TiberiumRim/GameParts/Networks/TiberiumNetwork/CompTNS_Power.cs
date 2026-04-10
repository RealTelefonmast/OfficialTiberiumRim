using TeleCore.Events.Args;

namespace TR.Networks.TiberiumNetwork;

public class CompTNS_Power : Comp_TiberiumNetworkStructure
{
    private CompPower_Tiberium _power;

    public new CompProperties_TNSPower Props => (CompProperties_TNSPower)base.props;

    public bool GeneratesPowerNow => _power?.GeneratesPowerNow ?? false;

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);
        _power = parent.GetComp<CompPower_Tiberium>();
    }

    public override bool? FX_ShouldDraw(FXLayerArgs args)
    {
        return args.index switch
        {
            1 => HasConnection,
            2 => GeneratesPowerNow,
            3 => GeneratesPowerNow,
            _ => true
        };
    }

    public override bool? FX_ShouldThrowEffects(FXEffecterArgs args)
    {
        return GeneratesPowerNow;
    }
}

public class CompProperties_TNSPower : CompProperties_TNS
{
    public int consumeAmt = 0;
    public float daysPerLoad = 1f;

    public CompProperties_TNSPower()
    {
        compClass = typeof(CompTNS_Power);
    }
}
