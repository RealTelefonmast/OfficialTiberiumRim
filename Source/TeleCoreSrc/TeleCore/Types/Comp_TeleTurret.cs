namespace TeleCore.Unsorted;

//TODO: Adjust to new Verse.CompTurretGun
/*public class Comp_TeleTurret : ThingComp, ITurretHolder, IAttackTarget, IAttackTargetSearcher
{
    //
    private TurretGunSet turretSet;

    public CompProperties_Turret Props => (CompProperties_Turret) props;

    //
    private Pawn ManningPawn => MannableComp?.ManningPawn;
    private bool MannedByColonist => ManningPawn?.Faction == Faction.OfPlayer;
    private bool MannedByNonColonist => ManningPawn?.Faction != Faction.OfPlayer;

    public bool IsStunned => false;

    public bool Spawned => parent.Spawned;
    /*{
        get
        {
            if (!this.triedGettingStunner)
            {
                CompStunnable comp = base.GetComp<CompStunnable>();
                this.stunner = ((comp != null) ? comp.StunHandler : null);
                this.triedGettingStunner = true;
            }
            return this.stunner != null && this.stunner.Stunned;
        }
    }#1#

    //
    public Thing Thing => parent;
    public LocalTargetInfo TargetCurrentlyAimingAt { get; }
    public float TargetPriorityFactor { get; }

    public string GetUniqueLoadID()
    {
        return parent.GetUniqueLoadID();
    }

    public Verb CurrentEffectiveVerb { get; }
    public LocalTargetInfo LastAttackedTarget { get; }
    public int LastAttackTargetTick { get; }

    //TurretHolder
    public LocalTargetInfo TargetOverride => LocalTargetInfo.Invalid;

    public bool IsActive => parent.Spawned && (PowerComp == null || PowerComp.PowerOn) &&
                            (MannableComp == null || MannableComp.MannedNow);

    public bool PlayerControlled => (Faction == Faction.OfPlayer || MannedByColonist) && !MannedByNonColonist;

    public Thing Caster => parent;
    public Thing HolderThing => parent;
    public Faction Faction => parent.Faction;
    public CompPowerTrader PowerComp { get; private set; }

    public CompCanBeDormant DormantComp { get; private set; }

    public CompInitiatable InitiatableComp { get; private set; }

    public CompMannable MannableComp { get; private set; }

    public CompRefuelable RefuelComp { get; private set; }

    public CompNetwork NetworkComp { get; private set; }

    public StunHandler Stunner { get; private set; }

    public void Notify_OnProjectileFired()
    {
    }

    public bool ThreatDisabled(IAttackTargetSearcher disabledFor)
    {
        if (PowerComp is {PowerOn: false}) return true;
        return MannableComp is {MannedNow: false};
    }


    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);
        //
        PowerComp = parent.GetComp<CompPowerTrader>();
        DormantComp = parent.GetComp<CompCanBeDormant>();
        InitiatableComp = parent.GetComp<CompInitiatable>();
        MannableComp = parent.GetComp<CompMannable>();
        RefuelComp = parent.GetComp<CompRefuelable>();
        NetworkComp = parent.GetComp<CompNetwork>();

        //
        Stunner = new StunHandler(parent);
        turretSet = new TurretGunSet(Props.turrets, this);
    }

    public override void CompTick()
    {
        turretSet.TickTurrets();
    }

    public override string CompInspectStringExtra()
    {
        return turretSet.InspectString();
    }

    public override void PostDraw()
    {
        turretSet.Draw();
    }
}

public class CompProperties_Turret : CompProperties
{
    public List<TurretProperties> turrets;

    public CompProperties_Turret()
    {
        compClass = typeof(Comp_TeleTurret);
    }
}*/