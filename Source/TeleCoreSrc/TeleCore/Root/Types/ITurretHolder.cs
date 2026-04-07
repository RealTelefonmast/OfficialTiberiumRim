using RimWorld;
using TeleCore.Systems.Turrets;
using TeleCore.ThingComps;
using Verse;
using Verse.AI;

namespace TeleCore.Types;

public interface ITurretHolder
{
    LocalTargetInfo TargetOverride { get; }
    bool Active { get; }
    bool PlayerControlled { get; }
    bool MannedByColonist { get; }
    bool IsStunned { get; }
    bool Spawned { get; }

    Thing Caster { get; }
    Thing HolderThing { get; }
    Faction Faction { get; }

    TurretGunSet TurretSet { get; }

    //ThingComps
    CompPowerTrader? PowerTraderComp { get; }
    CompCanBeDormant? DormantComp { get; }
    CompInitiatable? InitiatableComp { get; }
    CompMannable? MannableComp { get; }
    CompRefuelable? RefuelComp { get; }
    CompNetwork? NetworkComp { get; }

    void Notify_OnProjectileFired();
    bool ThreatDisabled(IAttackTargetSearcher disabledFor);
    void Notify_LostTarget(LocalTargetInfo forcedTarget);
    void Notify_NewTarget(LocalTargetInfo currentTarget);
}