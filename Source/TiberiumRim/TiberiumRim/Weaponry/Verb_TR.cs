using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using UnityEngine;

namespace TiberiumRim
{
    public class Verb_TR : Verb
    {
        public TurretGun castingGun;
        public CompTNW_Turret TiberiumComp => caster.TryGetComp<CompTNW_Turret>();
        public VerbProperties_TR Props => (VerbProperties_TR)base.verbProps;

        private int OffsetIndex => castingGun.ShotIndex;

        public ThingDef Projectile
        {
            get
            {
                if (EquipmentSource != null)
                {
                    CompChangeableProjectile comp = base.EquipmentSource.GetComp<CompChangeableProjectile>();
                    if (comp != null && comp.Loaded)
                    {
                        return comp.Projectile;
                    }
                }
                return verbProps.defaultProjectile;
            }
        }

        public Vector3 DrawPos
        {
            get
            {
                if(castingGun != null)
                {
                    return castingGun.DrawPos;
                }
                return caster.DrawPos;
            }
        }

        protected Vector3 NextOffset()
        {
            if (!Props.originOffsets.NullOrEmpty())
                return Props.originOffsets[OffsetIndex];
            return Vector3.zero;
        }

        protected Vector3 ShotOrigin()
        {
            Vector3 offset = Vector3.zero;
            if (castingGun != null && castingGun.top.props.barrelMuzzleOffset != Vector3.zero)
            {
                offset = castingGun.top.props.barrelMuzzleOffset;
            }
            offset += NextOffset();
            return DrawPos + offset.RotatedBy(GunRotation);
        }

        public bool IsMortar => !IsBeam && Props.defaultProjectile.projectile.flyOverhead;

        public bool IsBeam => Props.laser != null;

        protected override int ShotsPerBurst => this.verbProps.burstShotCount;

        protected float GunRotation { 
            get 
            {
                if (CasterIsPawn)
                {
                    Vector3 a;
                    float num = 0f;
                    Stance_Busy stance = CasterPawn.stances.curStance as Stance_Busy;
                    if (stance != null && !stance.neverAimWeapon && stance.focusTarg.IsValid)
                    {
                        if (stance.focusTarg.HasThing)
                        {
                            a = stance.focusTarg.Thing.DrawPos;
                        }
                        else
                        {
                            a = stance.focusTarg.Cell.ToVector3Shifted();
                        }
                        if ((a - CasterPawn.DrawPos).MagnitudeHorizontalSquared() > 0.001f)
                        {
                            num = (a - CasterPawn.DrawPos).AngleFlat();
                        }
                        return num;
                    }
                }
                Log.Message("Gun: " + (castingGun != null) + " " + (castingGun?.props != null));
                return castingGun != null ? (castingGun.HasTurret ? castingGun.TurretRotation : 0f) : 0f; 
            }
        }

        public override bool Available()
        {
            if (!base.Available())
                return false;

            if(Props.powerConsumption > 0)
            {

            }
            if (Props.tiberiumCostPerBurst != null)
            {
                return Props.tiberiumCostPerBurst.CanPay(TiberiumComp.Container);
            }
            if(Props.tiberiumCostPerShot != null)
            {
                return Props.tiberiumCostPerShot.CanPay(TiberiumComp.Container);
            }
            if (CasterIsPawn)
            {
                Pawn casterPawn = base.CasterPawn;
                if (casterPawn.Faction != Faction.OfPlayer && casterPawn.mindState.MeleeThreatStillThreat && casterPawn.mindState.meleeThreat.Position.AdjacentTo8WayOrInside(casterPawn.Position))
                {
                    return false;
                }
            }
            return IsBeam ? true : Projectile != null;
        }

        public override void WarmupComplete()
        {
            base.WarmupComplete();
            if (Props.tiberiumCostPerBurst != null)
            {
                Props.tiberiumCostPerBurst.Pay(TiberiumComp.Container);
            }
        }     

        protected override bool TryCastShot()
        {
            if (IsBeam)
            {
                return TryCastBeam();
            }
            bool flag = TryCastProjectile();
            if(flag && Props.tiberiumCostPerShot != null)
            {
                if (Props.tiberiumCostPerShot.CanPay(TiberiumComp.Container))
                    Props.tiberiumCostPerShot.Pay(TiberiumComp.Container);
                else
                    return false;
            }
            if (flag && base.CasterIsPawn)
            {
                base.CasterPawn.records.Increment(RecordDefOf.ShotsFired);
            }
            return flag;
        }

        public bool TryCastProjectile()
        {
            if (this.currentTarget.HasThing && this.currentTarget.Thing.Map != this.caster.Map)
                return false;

            ThingDef projectile = this.Projectile;
            if (projectile == null)
                return false;

            ShootLine shootLine;
            bool flag = base.TryFindShootLineFromTo(this.caster.Position, this.currentTarget, out shootLine);
            if (this.verbProps.stopBurstWithoutLos && !flag)
                return false;

            castingGun.Notify_FiredSingleProjectile();
            if (base.EquipmentSource != null)
            {
                CompChangeableProjectile comp = base.EquipmentSource.GetComp<CompChangeableProjectile>();
                if (comp != null)
                {
                    comp.Notify_ProjectileLaunched();
                }
            }
            Thing launcher = this.caster;
            Thing equipment = base.EquipmentSource;
            CompMannable compMannable = this.caster.TryGetComp<CompMannable>();
            if (compMannable != null && compMannable.ManningPawn != null)
            {
                launcher = compMannable.ManningPawn;
                equipment = this.caster;
            }
            Vector3 drawPos = ShotOrigin();
            Log.Message("Base DrawPos: " + DrawPos + " offset: " + drawPos);
            Projectile projectile2 = (Projectile)GenSpawn.Spawn(projectile, shootLine.Source, this.caster.Map, WipeMode.Vanish);
            if (this.verbProps.forcedMissRadius > 0.5f)
            {
                float num = VerbUtility.CalculateAdjustedForcedMiss(this.verbProps.forcedMissRadius, this.currentTarget.Cell - this.caster.Position);
                if (num > 0.5f)
                {
                    int max = GenRadial.NumCellsInRadius(num);
                    int num2 = Rand.Range(0, max);
                    if (num2 > 0)
                    {
                        IntVec3 c = this.currentTarget.Cell + GenRadial.RadialPattern[num2];
                        ProjectileHitFlags projectileHitFlags = ProjectileHitFlags.NonTargetWorld;
                        if (Rand.Chance(0.5f))
                        {
                            projectileHitFlags = ProjectileHitFlags.All;
                        }
                        if (!this.canHitNonTargetPawnsNow)
                        {
                            projectileHitFlags &= ~ProjectileHitFlags.NonTargetPawns;
                        }
                        projectile2.Launch(launcher, drawPos, c, this.currentTarget, projectileHitFlags, equipment, null);
                        return true;
                    }
                }
            }
            ShotReport shotReport = ShotReport.HitReportFor(this.caster, this, this.currentTarget);
            Thing randomCoverToMissInto = shotReport.GetRandomCoverToMissInto();
            ThingDef targetCoverDef = (randomCoverToMissInto == null) ? null : randomCoverToMissInto.def;
            if (!Rand.Chance(shotReport.AimOnTargetChance_IgnoringPosture))
            {
                shootLine.ChangeDestToMissWild(shotReport.AimOnTargetChance_StandardTarget);
                ProjectileHitFlags projectileHitFlags2 = ProjectileHitFlags.NonTargetWorld;
                if (Rand.Chance(0.5f) && this.canHitNonTargetPawnsNow)
                {
                    projectileHitFlags2 |= ProjectileHitFlags.NonTargetPawns;
                }
                projectile2.Launch(launcher, drawPos, shootLine.Dest, this.currentTarget, projectileHitFlags2, equipment, targetCoverDef);
                return true;
            }
            if (this.currentTarget.Thing != null && this.currentTarget.Thing.def.category == ThingCategory.Pawn && !Rand.Chance(shotReport.PassCoverChance))
            {
                ProjectileHitFlags projectileHitFlags3 = ProjectileHitFlags.NonTargetWorld;
                if (this.canHitNonTargetPawnsNow)
                {
                    projectileHitFlags3 |= ProjectileHitFlags.NonTargetPawns;
                }
                projectile2.Launch(launcher, drawPos, randomCoverToMissInto, this.currentTarget, projectileHitFlags3, equipment, targetCoverDef);
                return true;
            }
            ProjectileHitFlags projectileHitFlags4 = ProjectileHitFlags.IntendedTarget;
            if (this.canHitNonTargetPawnsNow)
            {
                projectileHitFlags4 |= ProjectileHitFlags.NonTargetPawns;
            }
            if (!this.currentTarget.HasThing || this.currentTarget.Thing.def.Fillage == FillCategory.Full)
            {
                projectileHitFlags4 |= ProjectileHitFlags.NonTargetWorld;
            }
            if (this.currentTarget.Thing != null)
            {
                projectile2.Launch(launcher, drawPos, this.currentTarget, this.currentTarget, projectileHitFlags4, equipment, targetCoverDef);
            }
            else
            {
                projectile2.Launch(launcher, drawPos, shootLine.Dest, this.currentTarget, projectileHitFlags4, equipment, targetCoverDef);
            }
            return true;
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

        public override float HighlightFieldRadiusAroundTarget(out bool needLOSToCenter)
        {
            needLOSToCenter = true;
            ThingDef projectile = this.Projectile;
            if (projectile == null)
            {
                return 0f;
            }
            return projectile.projectile.explosionRadius;
        }

    }

    public class VerbProperties_TR : VerbProperties
    {
        public List<Vector3> originOffsets;
        public TiberiumCost tiberiumCostPerBurst;
        public TiberiumCost tiberiumCostPerShot;
        public SoundDef chargeSound;
        public float powerConsumption = 0;

        public LaserProperties laser;
    }
}
