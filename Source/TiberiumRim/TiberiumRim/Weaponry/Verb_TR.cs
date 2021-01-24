using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.Sound;
using Verse.AI;
using UnityEngine;

namespace TiberiumRim
{
    public class Verb_TR : Verb
    {
        //This custom verb replaces and reworks most of the base Verb, often doing redundant things to avoid complicated specific patches.

        //Shot Index
        private int lastOffsetIndex = 0;
        private int offsetIndex = 0;
        private int maxOffsetCount = 1;

        public TurretGun castingGun;
        public CompTNW_Turret TiberiumComp => caster.TryGetComp<CompTNW_Turret>();
        public VerbProperties_TR Props => (VerbProperties_TR)verbProps;

        private int OffsetIndex => castingGun?.ShotIndex ?? offsetIndex;

        private ThingDef currentProjectile;

        public ThingDef Projectile
        {
            get
            {
                CompChangeableProjectile comp = EquipmentSource?.GetComp<CompChangeableProjectile>();
                if (comp != null && comp.Loaded)
                {
                    return comp.Projectile;
                }
                if (currentProjectile == null)
                    currentProjectile = Props.defaultProjectile;
                return currentProjectile; 
            }
            set => currentProjectile = value;
        }

        public Vector3 DrawPos => castingGun?.DrawPos ?? caster.DrawPos;

        private void Notify_SingleShot()
        {
            if(castingGun != null)
                castingGun.Notify_FiredSingleProjectile();
            else
                RotateNextShotIndex();
        }

        private void RotateNextShotIndex()
        {
            lastOffsetIndex = offsetIndex;
            offsetIndex++;
            if (offsetIndex > (maxOffsetCount - 1))
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
            Vector3 offset = Vector3.zero;
            if (castingGun?.top != null && castingGun.top.props.barrelMuzzleOffset != Vector3.zero)
            {
                offset = castingGun.top.props.barrelMuzzleOffset;
            }
            offset += NextOffset();
            return DrawPos + offset.RotatedBy(GunRotation);
        }

        public bool IsMortar => !IsBeam && Props.defaultProjectile.projectile.flyOverhead;

        public bool IsBeam => Props.beamProps != null;

        protected override int ShotsPerBurst => this.verbProps.burstShotCount;

        

        protected float GunRotation
        {
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
                return castingGun != null ? (castingGun.HasTurret ? castingGun.TurretRotation : 0f) : 0f;
            }
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
                Pawn pawn = currentTarget.Thing as Pawn;
                if (pawn != null && pawn.IsColonistPlayerControlled)
                {
                    CasterPawn.records.AccumulateStoryEvent(StoryEventDefOf.AttackedPlayer);
                }
            }
            Props.tiberiumCostPerBurst?.Pay(TiberiumComp.Container);
        }

        protected override bool TryCastShot()
        {
            var flag = IsBeam ? TryCastBeam() : TryCastProjectile();

            if (flag)
                Notify_SingleShot();

            if (flag && Props.tiberiumCostPerShot != null)
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
                Pawn casterPawn = CasterPawn;
                if (casterPawn.Faction != Faction.OfPlayer && casterPawn.mindState.MeleeThreatStillThreat && casterPawn.mindState.meleeThreat.Position.AdjacentTo8WayOrInside(casterPawn.Position))
                {
                    return false;
                }
            }
            return IsBeam || Projectile != null;
        }

        public void CastProjectile(IntVec3 origin, Thing caster, LocalTargetInfo usedTarget, LocalTargetInfo intendedTarget, ProjectileHitFlags flags)
        {
            Projectile projectile = (Projectile)GenSpawn.Spawn(Projectile, origin, caster.Map, WipeMode.Vanish);
            projectile.Launch(caster, usedTarget, intendedTarget, flags);
        }

        public bool TryCastProjectile()
        {
            if (currentTarget.HasThing && currentTarget.Thing.Map != caster.Map)
                return false;

            ThingDef projectile = Projectile;
            if (projectile == null)
                return false;

            ShootLine shootLine;
            bool flag = TryFindShootLineFromTo(caster.Position, currentTarget, out shootLine);
            if (verbProps.stopBurstWithoutLos && !flag)
                return false;

            if (EquipmentSource != null)
            {
                CompChangeableProjectile comp = EquipmentSource.GetComp<CompChangeableProjectile>();
                comp?.Notify_ProjectileLaunched();
            }
            Thing launcher = caster;
            Thing equipment = EquipmentSource;
            CompMannable compMannable = caster.TryGetComp<CompMannable>();
            if (compMannable != null && compMannable.ManningPawn != null)
            {
                launcher = compMannable.ManningPawn;
                equipment = caster;
            }
            Vector3 drawPos = ShotOrigin();
            Projectile projectile2 = (Projectile)GenSpawn.Spawn(projectile, shootLine.Source, caster.Map, WipeMode.Vanish);
            if (verbProps.forcedMissRadius > 0.5f)
            {
                float num = VerbUtility.CalculateAdjustedForcedMiss(verbProps.forcedMissRadius, currentTarget.Cell - caster.Position);
                if (num > 0.5f)
                {
                    int max = GenRadial.NumCellsInRadius(num);
                    int num2 = Rand.Range(0, max);
                    if (num2 > 0)
                    {
                        IntVec3 c = currentTarget.Cell + GenRadial.RadialPattern[num2];
                        ProjectileHitFlags projectileHitFlags = ProjectileHitFlags.NonTargetWorld;
                        if (Rand.Chance(0.5f))
                        {
                            projectileHitFlags = ProjectileHitFlags.All;
                        }
                        if (!canHitNonTargetPawnsNow)
                        {
                            projectileHitFlags &= ~ProjectileHitFlags.NonTargetPawns;
                        }
                        projectile2.Launch(launcher, drawPos, c, currentTarget, projectileHitFlags, equipment, null);
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
            if (currentTarget.Thing != null && currentTarget.Thing.def.category == ThingCategory.Pawn && !Rand.Chance(shotReport.PassCoverChance))
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

        public void SwitchProjectile()
        {
            if (Projectile == Props.defaultProjectile)
            {
                Projectile = Props.secondaryProjectile;
                return;
            }

            if (Projectile == Props.secondaryProjectile)
            {
                Projectile = Props.defaultProjectile;
                return;
            }
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

        public LocalTargetInfo AdjustedTarget(LocalTargetInfo intended, ref ShootLine shootLine, out ProjectileHitFlags flags)
        {
            flags = ProjectileHitFlags.NonTargetWorld;
            if (verbProps.forcedMissRadius > 0.5f)
            {
                float num = VerbUtility.CalculateAdjustedForcedMiss(verbProps.forcedMissRadius, intended.Cell - caster.Position);
                if (num > 0.5f)
                {
                    if (Rand.Chance(0.5f))
                        flags = ProjectileHitFlags.All;
                    if (!canHitNonTargetPawnsNow)
                        flags &= ~ProjectileHitFlags.NonTargetPawns;
                    
                    int max = GenRadial.NumCellsInRadius(num);
                    int num2 = Rand.Range(0, max);
                    if (num2 > 0)
                    {
                        return GetTargetFromPos((intended.Cell + GenRadial.RadialPattern[num2]), caster.Map);
                    }
                }
            }
            ShotReport shotReport = ShotReport.HitReportFor(caster, this, intended);
            Thing cover = shotReport.GetRandomCoverToMissInto();
            if (!Rand.Chance(shotReport.AimOnTargetChance_IgnoringPosture))
            {
                if (Rand.Chance(0.5f) && canHitNonTargetPawnsNow)
                    flags |= ProjectileHitFlags.NonTargetPawns;
                shootLine.ChangeDestToMissWild(shotReport.AimOnTargetChance_StandardTarget);
                return GetTargetFromPos(shootLine.Dest, caster.Map);
            }
            if (intended.Thing != null && intended.Thing.def.category == ThingCategory.Pawn && !Rand.Chance(shotReport.PassCoverChance))
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
            ThingDef projectile = this.Projectile;
            if (projectile == null)
            {
                return 0f;
            }
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
        public string label;
        public string description;

        public VerbBurstMode mode = VerbBurstMode.Normal;
        public ThingDef secondaryProjectile;

        public List<Vector3> originOffsets;
        public TiberiumCost tiberiumCostPerBurst;
        public TiberiumCost tiberiumCostPerShot;
        public SoundDef chargeSound;
        public float powerConsumption = 0;
        public int shotIntervalTicks = 10;
        public BeamProperties beamProps;
    }
}
