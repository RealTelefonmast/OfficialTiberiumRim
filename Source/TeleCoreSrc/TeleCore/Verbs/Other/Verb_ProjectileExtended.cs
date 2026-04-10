using RimWorld;
using TeleCore.Visual.VFX.Particles.Motes;
using Verse;

namespace TeleCore.Verbs.Other;

public class Verb_ProjectileExtended : Verb_Tele
{
    //
    private ThingDef currentProjectile;

    //
    public override ThingDef Projectile
    {
        get
        {
            var comp = EquipmentSource?.GetComp<CompChangeableProjectile>();
            if (comp is { Loaded: true }) return comp.Projectile;
            return currentProjectile ??= Props.defaultProjectile;
        }
    }

    public override DamageDef DamageDef => Projectile.projectile.damageDef;

    protected override float ExplosionOnTargetSize => Projectile.projectile.explosionRadius;

    protected override BattleLogEntry_RangedFire EntryOnWarmupComplete()
    {
        return new BattleLogEntry_RangedFire(caster, currentTarget.HasThing ? currentTarget.Thing : null,
            EquipmentSource != null ? EquipmentSource.def : null, Projectile, ShotsPerBurst > 1);
    }

    public void SetProjectile(ThingDef projectile)
    {
        currentProjectile = projectile;
    }

    protected override bool IsAvailable()
    {
        return Projectile != null;
    }

    protected override bool TryCastAttack()
    {
        if (currentTarget.HasThing && currentTarget.Thing.Map != caster.Map) return false;

        var projectile = Projectile;
        if (projectile == null) return false;

        var flag = TryFindShootLineFromTo(caster.Position, currentTarget, out var shootLine);
        if (verbProps.stopBurstWithoutLos && !flag) return false;

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

        var drawPos = CurrentStartPos;
        //Projectile projectile2 = (Projectile)GenSpawn.Spawn(projectile, shootLine.Source, caster.Map, WipeMode.Vanish);
        if (verbProps.ForcedMissRadius > 0.5f)
        {
            var num = VerbUtility.CalculateAdjustedForcedMiss(verbProps.ForcedMissRadius,
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
                    CastProjectile(shootLine.Source, launcher, drawPos, c, currentTarget, projectileHitFlags,
                        Props.avoidFriendlyFire, equipment, null);
                    //projectile2.Launch(launcher, drawPos, c, currentTarget, projectileHitFlags, Props.avoidFriendlyFire, equipment, null);
                    return true;
                }
            }
        }

        var shotReport = ShotReport.HitReportFor(caster, this, currentTarget);
        var randomCoverToMissInto = shotReport.GetRandomCoverToMissInto();
        var targetCoverDef = randomCoverToMissInto?.def;
        if (!Rand.Chance(shotReport.AimOnTargetChance_IgnoringPosture))
        {
            shootLine.ChangeDestToMissWild(shotReport.AimOnTargetChance_StandardTarget, caster.Map);
            var projectileHitFlags2 = ProjectileHitFlags.NonTargetWorld;
            if (Rand.Chance(0.5f) && canHitNonTargetPawnsNow) projectileHitFlags2 |= ProjectileHitFlags.NonTargetPawns;
            CastProjectile(shootLine.Source, launcher, drawPos, shootLine.Dest, currentTarget, projectileHitFlags2,
                Props.avoidFriendlyFire, equipment, targetCoverDef);
            return true;
        }

        if (currentTarget.Thing != null && currentTarget.Thing.def.category == ThingCategory.Pawn &&
            !Rand.Chance(shotReport.PassCoverChance))
        {
            var projectileHitFlags3 = ProjectileHitFlags.NonTargetWorld;
            if (canHitNonTargetPawnsNow) projectileHitFlags3 |= ProjectileHitFlags.NonTargetPawns;
            CastProjectile(shootLine.Source, launcher, drawPos, randomCoverToMissInto, currentTarget,
                projectileHitFlags3, Props.avoidFriendlyFire, equipment, targetCoverDef);
            return true;
        }

        var projectileHitFlags4 = ProjectileHitFlags.IntendedTarget;
        if (canHitNonTargetPawnsNow) projectileHitFlags4 |= ProjectileHitFlags.NonTargetPawns;
        if (!currentTarget.HasThing || currentTarget.Thing.def.Fillage == FillCategory.Full)
            projectileHitFlags4 |= ProjectileHitFlags.NonTargetWorld;
        if (currentTarget.Thing != null)
            CastProjectile(shootLine.Source, launcher, drawPos, currentTarget, currentTarget, projectileHitFlags4,
                Props.avoidFriendlyFire, equipment, targetCoverDef);
        else
            CastProjectile(shootLine.Source, launcher, drawPos, shootLine.Dest, currentTarget, projectileHitFlags4,
                Props.avoidFriendlyFire, equipment, targetCoverDef);
        return true;
    }

    public void CastProjectile(IntVec3 origin, Thing caster, Vector3 drawPos, LocalTargetInfo usedTarget,
        LocalTargetInfo intendedTarget, ProjectileHitFlags flags, bool avoidFriendly, Thing equipmentSource,
        ThingDef targetCoverDef)
    {
        var projectile = (Projectile) GenSpawn.Spawn(Projectile, origin, caster.Map);
        projectile.Launch(caster, drawPos, usedTarget, intendedTarget, flags, avoidFriendly, equipmentSource,
            targetCoverDef);
        DoMuzzleFlash(drawPos, intendedTarget);
    }

    //
    private void DoMuzzleFlash(Vector3 origin, LocalTargetInfo intendedTarget)
    {
        var flash = Props.muzzleFlash;
        if (flash == null) return;
        var flashMote = (Mote_MuzzleFlash) ThingMaker.MakeThing(TeleDefOf.Mote_MuzzleFlash);
        flashMote.Scale = flash.scale;
        flashMote.solidTimeOverride = flash.solidTime;
        flashMote.fadeInTimeOverride = flash.fadeInTime;
        flashMote.fadeOutTimeOverride = flash.fadeOutTime;
        flashMote.AttachMaterial(flash.Graphic.MatSingle, Color.white);
        flashMote.SetLookDirection(origin, intendedTarget.CenterVector3);
        flashMote.Attach(caster);
        GenSpawn.Spawn(flashMote, caster.Position, caster.Map);
    }
}