using System.Collections.Generic;
using RimWorld;
using TeleCore.Network.Bills;
using TR.Networks.TiberiumNetwork;
using UnityEngine;
using Verse;

namespace TR;

public class Verb_TR : Verb
{
    public TurretGun castingGun;

    private ThingDef currentProjectile;
    //This custom verb replaces and reworks most of the base Verb, often doing redundant things to avoid complicated specific patches.

    //Shot Index
    private int lastOffsetIndex;
    private int maxOffsetCount = 1;
    private int offsetIndex;
    public CompTNS_Turret TiberiumComp => caster.TryGetComp<CompTNS_Turret>();
    public VerbProperties_TR Props => (VerbProperties_TR)verbProps;

    private int OffsetIndex => castingGun?.ShotIndex ?? offsetIndex;

    public ThingDef Projectile
    {
        get
        {
            var comp = EquipmentSource?.GetComp<CompChangeableProjectile>();
            if (comp != null && comp.Loaded) return comp.Projectile;
            if (currentProjectile == null)
                currentProjectile = Props.defaultProjectile;
            return currentProjectile;
        }
        set => currentProjectile = value;
    }

    public Vector3 DrawPos => castingGun?.DrawPos ?? caster.DrawPos;

    public bool IsMortar => !IsBeam && Props.defaultProjectile.projectile.flyOverhead;

    public bool IsBeam => Props.beamProps != null;

    public override int ShotsPerBurst => verbProps.burstShotCount;


    protected float GunRotation
    {
        get
        {
            if (CasterIsPawn)
            {
                Vector3 a;
                var num = 0f;
                var stance = CasterPawn.stances.curStance as Stance_Busy;
                if (stance != null && !stance.neverAimWeapon && stance.focusTarg.IsValid)
                {
                    if (stance.focusTarg.HasThing)
                        a = stance.focusTarg.Thing.DrawPos;
                    else
                        a = stance.focusTarg.Cell.ToVector3Shifted();

                    if ((a - CasterPawn.DrawPos).MagnitudeHorizontalSquared() > 0.001f)
                        num = (a - CasterPawn.DrawPos).AngleFlat();

                    return num;
                }
            }

            return castingGun != null ? castingGun.HasTurret ? castingGun.TurretRotation : 0f : 0f;
        }
    }

    private void Notify_SingleShot()
    {
        if (castingGun != null)
            castingGun.Notify_FiredSingleProjectile();
        else
            RotateNextShotIndex();
    }

    private void RotateNextShotIndex()
    {
        lastOffsetIndex = offsetIndex;
        offsetIndex++;
        if (offsetIndex > maxOffsetCount - 1)
            offsetIndex = 0;
    }

    protected Vector3 NextOffset()
    {
        if (!Props.originOffsets.NullOrEmpty())
            return Props.originOffsets[OffsetIndex];
        return Vector3.zero;
    }

    protected Vector3 ShotOrigin()
    {
        var offset = Vector3.zero;
        if (castingGun?.top != null && castingGun.top.props.barrelMuzzleOffset != Vector3.zero)
            offset = castingGun.top.props.barrelMuzzleOffset;
        offset += NextOffset();
        return DrawPos + offset.RotatedBy(GunRotation);
    }

    public override bool IsUsableOn(Thing target)
    {
        return true;
    }

    public override void ExposeData()
    {
        base.ExposeData();
    }

    public virtual void CustomTick()
    {
    }

    public override void Reset()
    {
        base.Reset();
        maxOffsetCount = Props.originOffsets?.Count ?? 0;
    }

    public override void WarmupComplete()
    {
        burstShotsLeft = ShotsPerBurst;
        state = VerbState.Bursting;
        TryCastNextBurstShot();
        if (CasterIsPawn && currentTarget.HasThing)
        {
            var pawn = currentTarget.Thing as Pawn;
            if (pawn is { IsColonistPlayerControlled: true })
                CasterPawn.records.AccumulateStoryEvent(StoryEventDefOf.AttackedPlayer);
        }

        Props.tiberiumCostPerBurst?.DoPayWith(TiberiumComp);
    }

    public override bool TryCastShot()
    {
        var flag = IsBeam ? TryCastBeam() : TryCastProjectile();

        if (flag)
            Notify_SingleShot();

        if (flag && Props.tiberiumCostPerShot != null)
        {
            if (Props.tiberiumCostPerShot.CanPayWith(TiberiumComp.TiberiumNetPart))
                Props.tiberiumCostPerShot.DoPayWith(TiberiumComp);
            else
                return false;
        }

        if (flag && base.CasterIsPawn) base.CasterPawn.records.Increment(RecordDefOf.ShotsFired);
        return flag;
    }


    public override bool Available()
    {
        if (!base.Available())
            return false;

        if (Props.powerConsumption > 0)
        {
        }

        if (Props.tiberiumCostPerBurst != null) return Props.tiberiumCostPerBurst.CanPayWith(TiberiumComp.TiberiumNetPart);
        if (Props.tiberiumCostPerShot != null) return Props.tiberiumCostPerShot.CanPayWith(TiberiumComp.TiberiumNetPart);
        if (CasterIsPawn)
        {
            var casterPawn = CasterPawn;
            if (casterPawn.Faction != Faction.OfPlayer && casterPawn.mindState.MeleeThreatStillThreat &&
                casterPawn.mindState.meleeThreat.Position.AdjacentTo8WayOrInside(casterPawn.Position)) return false;
        }

        return IsBeam || Projectile != null;
    }

    public void CastProjectile(IntVec3 origin, Thing caster, LocalTargetInfo usedTarget, LocalTargetInfo intendedTarget,
        ProjectileHitFlags flags)
    {
        var projectile = (Projectile)GenSpawn.Spawn(Projectile, origin, caster.Map);
        projectile.Launch(caster, usedTarget, intendedTarget, flags);
    }

    public bool TryCastProjectile()
    {
        if (currentTarget.HasThing && currentTarget.Thing.Map != caster.Map)
            return false;

        var projectile = Projectile;
        if (projectile == null)
            return false;

        ShootLine shootLine;
        var flag = TryFindShootLineFromTo(caster.Position, currentTarget, out shootLine);
        if (verbProps.stopBurstWithoutLos && !flag)
            return false;

        if (EquipmentSource != null)
        {
            var comp = EquipmentSource.GetComp<CompChangeableProjectile>();
            comp?.Notify_ProjectileLaunched();
        }

        var launcher = caster;
        Thing equipment = EquipmentSource;
        var compMannable = caster.TryGetComp<CompMannable>();
        if (compMannable != null && compMannable.ManningPawn != null)
        {
            launcher = compMannable.ManningPawn;
            equipment = caster;
        }

        var drawPos = ShotOrigin();
        var projectile2 = (Projectile)GenSpawn.Spawn(projectile, shootLine.Source, caster.Map);
        if (verbProps.forcedMissRadius > 0.5f)
        {
            var num = VerbUtility.CalculateAdjustedForcedMiss(verbProps.forcedMissRadius,
                currentTarget.Cell - caster.Position);
            if (num > 0.5f)
            {
                var max = GenRadial.NumCellsInRadius(num);
                var num2 = Rand.Range(0, max);
                if (num2 > 0)
                {
                    var c = currentTarget.Cell + GenRadial.RadialPattern[num2];
                    var projectileHitFlags = ProjectileHitFlags.NonTargetWorld;
                    if (Rand.Chance(0.5f)) projectileHitFlags = ProjectileHitFlags.All;
                    if (!canHitNonTargetPawnsNow) projectileHitFlags &= ~ProjectileHitFlags.NonTargetPawns;
                    projectile2.Launch(launcher, drawPos, c, currentTarget, projectileHitFlags, equipment, null);
                    return true;
                }
            }
        }

        var shotReport = ShotReport.HitReportFor(caster, this, currentTarget);
        var randomCoverToMissInto = shotReport.GetRandomCoverToMissInto();
        var targetCoverDef = randomCoverToMissInto == null ? null : randomCoverToMissInto.def;
        if (!Rand.Chance(shotReport.AimOnTargetChance_IgnoringPosture))
        {
            shootLine.ChangeDestToMissWild(shotReport.AimOnTargetChance_StandardTarget);
            var projectileHitFlags2 = ProjectileHitFlags.NonTargetWorld;
            if (Rand.Chance(0.5f) && canHitNonTargetPawnsNow) projectileHitFlags2 |= ProjectileHitFlags.NonTargetPawns;
            projectile2.Launch(launcher, drawPos, shootLine.Dest, currentTarget, projectileHitFlags2, equipment,
                targetCoverDef);
            return true;
        }

        if (currentTarget.Thing != null && currentTarget.Thing.def.category == ThingCategory.Pawn &&
            !Rand.Chance(shotReport.PassCoverChance))
        {
            var projectileHitFlags3 = ProjectileHitFlags.NonTargetWorld;
            if (canHitNonTargetPawnsNow) projectileHitFlags3 |= ProjectileHitFlags.NonTargetPawns;
            projectile2.Launch(launcher, drawPos, randomCoverToMissInto, currentTarget, projectileHitFlags3, equipment,
                targetCoverDef);
            return true;
        }

        var projectileHitFlags4 = ProjectileHitFlags.IntendedTarget;
        if (canHitNonTargetPawnsNow) projectileHitFlags4 |= ProjectileHitFlags.NonTargetPawns;
        if (!currentTarget.HasThing || currentTarget.Thing.def.Fillage == FillCategory.Full)
            projectileHitFlags4 |= ProjectileHitFlags.NonTargetWorld;
        if (currentTarget.Thing != null)
            projectile2.Launch(launcher, drawPos, currentTarget, currentTarget, projectileHitFlags4, equipment,
                targetCoverDef);
        else
            projectile2.Launch(launcher, drawPos, shootLine.Dest, currentTarget, projectileHitFlags4, equipment,
                targetCoverDef);
        return true;
    }

    public void SwitchProjectile()
    {
        if (Projectile == Props.defaultProjectile)
        {
            Projectile = Props.secondaryProjectile;
            return;
        }

        if (Projectile == Props.secondaryProjectile) Projectile = Props.defaultProjectile;
    }

    public virtual bool TryCastBeam()
    {
        Log.Error("Trying to cast beam without using Verb_Beam");
        return false;
    }

    public bool TryCastTiberium()
    {
        return true;
    }

    public LocalTargetInfo AdjustedTarget(LocalTargetInfo intended, ref ShootLine shootLine,
        out ProjectileHitFlags flags)
    {
        flags = ProjectileHitFlags.NonTargetWorld;
        if (verbProps.forcedMissRadius > 0.5f)
        {
            var num = VerbUtility.CalculateAdjustedForcedMiss(verbProps.forcedMissRadius,
                intended.Cell - caster.Position);
            if (num > 0.5f)
            {
                if (Rand.Chance(0.5f))
                    flags = ProjectileHitFlags.All;
                if (!canHitNonTargetPawnsNow)
                    flags &= ~ProjectileHitFlags.NonTargetPawns;

                var max = GenRadial.NumCellsInRadius(num);
                var num2 = Rand.Range(0, max);
                if (num2 > 0) return GetTargetFromPos(intended.Cell + GenRadial.RadialPattern[num2], caster.Map);
            }
        }

        var shotReport = ShotReport.HitReportFor(caster, this, intended);
        var cover = shotReport.GetRandomCoverToMissInto();
        if (!Rand.Chance(shotReport.AimOnTargetChance_IgnoringPosture))
        {
            if (Rand.Chance(0.5f) && canHitNonTargetPawnsNow)
                flags |= ProjectileHitFlags.NonTargetPawns;
            shootLine.ChangeDestToMissWild(shotReport.AimOnTargetChance_StandardTarget);
            return GetTargetFromPos(shootLine.Dest, caster.Map);
        }

        if (intended.Thing != null && intended.Thing.def.category == ThingCategory.Pawn &&
            !Rand.Chance(shotReport.PassCoverChance))
        {
            if (canHitNonTargetPawnsNow)
                flags |= ProjectileHitFlags.NonTargetPawns;
            return cover;
        }

        return intended;
    }

    private LocalTargetInfo GetTargetFromPos(IntVec3 pos, Map map)
    {
        var things = pos.GetThingList(map);
        if (things.NullOrEmpty()) return pos;
        return things.MaxBy(t => t.def.altitudeLayer);
    }

    public override float HighlightFieldRadiusAroundTarget(out bool needLOSToCenter)
    {
        needLOSToCenter = true;
        var projectile = Projectile;
        if (projectile == null) return 0f;
        return projectile.projectile.explosionRadius;
    }
}

public enum VerbBurstMode
{
    Normal,
    ToTarget
}

public class VerbProperties_TR : VerbProperties
{
    public BeamProperties beamProps;
    public SoundDef chargeSound;
    public string description;
    public string label;

    public VerbBurstMode mode = VerbBurstMode.Normal;

    public List<Vector3> originOffsets;
    public float powerConsumption = 0;
    public ThingDef secondaryProjectile;
    public int shotIntervalTicks = 10;
    public NetworkCost tiberiumCostPerBurst;
    public NetworkCost tiberiumCostPerShot;
}