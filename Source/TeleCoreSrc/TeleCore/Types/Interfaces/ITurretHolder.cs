using RimWorld;
using Verse;
using Verse.AI;

namespace TeleCore.Types.Interfaces;

public interface ITurretHolder
{
    LocalTargetInfo TargetOverride { get; }
    bool IsActive { get; }
    bool PlayerControlled { get; }
    bool MannedByColonist { get; }
    bool IsStunned { get; }
    bool Spawned { get; }

    Thing Caster { get; }
    Thing HolderThing { get; }
    Faction Faction { get; }

    //
    CompPowerTrader PowerComp { get; }
    CompCanBeDormant DormantComp { get; }
    CompInitiatable InitiatableComp { get; }

    //ThingComps
    CompPowerTrader? PowerTraderComp { get; }
    CompCanBeDormant? DormantComp { get; }
    CompInitiatable? InitiatableComp { get; }
    CompMannable? MannableComp { get; }
    CompRefuelable? RefuelComp { get; }
    CompNetwork? NetworkComp { get; }

    // 
    CompRefuelable RefuelComp { get; }
    Comp_Network NetworkComp { get; }
    StunHandler Stunner { get; }

    void Notify_OnProjectileFired();
    bool ThreatDisabled(IAttackTargetSearcher disabledFor);
    void Notify_LostTarget(LocalTargetInfo forcedTarget);
    void Notify_NewTarget(LocalTargetInfo currentTarget);
}