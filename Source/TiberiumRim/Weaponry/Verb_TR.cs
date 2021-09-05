using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    //This custom verb replaces and reworks most of the base Verb, often doing redundant things to avoid complicated specific patches.
    public class Verb_TR : Verb
    {
        //Shot Index
        private int lastOffsetIndex = 0;
        private int offsetIndex = 0;
        private int maxOffsetCount = 1;

        public TurretGun castingGun;
        public CompTNS_Turret TiberiumComp => caster.TryGetComp<CompTNS_Turret>();
        public VerbProperties_TR Props => (VerbProperties_TR)verbProps;

        private int OffsetIndex => castingGun?.ShotIndex ?? offsetIndex;

        private ThingDef currentProjectile;

        public virtual ThingDef Projectile
        {
            get
            {
                CompChangeableProjectile comp = EquipmentSource?.GetComp<CompChangeableProjectile>();
                if (comp is {Loaded: true})
                {
                    return comp.Projectile;
                }
                return currentProjectile ??= Props.defaultProjectile;
            }
        }

        public virtual DamageDef DamageDef => IsBeam ? Props.beamProps.damageDef : Projectile.projectile.damageDef;

        public void SetProjectile(ThingDef projectile) => currentProjectile = projectile;

        public Vector3 DrawPos => castingGun?.DrawPos ?? caster.DrawPos;

        protected Vector3 CurrentShotOffset
        {
            get
            {
                if (!Props.originOffsets.NullOrEmpty())
                    return Props.originOffsets[OffsetIndex];
                return Vector3.zero;
            }
        }

        protected Vector3 ShotOrigin
        {
            get
            {
                Vector3 offset = Vector3.zero;
                if (castingGun?.top != null && castingGun.top.props.barrelMuzzleOffset != Vector3.zero)
                {
                    offset = castingGun.top.props.barrelMuzzleOffset;
                }

                offset += CurrentShotOffset;
                return DrawPos + offset.RotatedBy(GunRotation);
            }
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

        private void Notify_SingleShot()
        {
            if (castingGun != null)
                castingGun.Notify_FiredSingleProjectile();
            else
                RotateNextShotIndex();
        }

        private void DoMuzzleFlash(Vector3 origin, LocalTargetInfo intendedTarget)
        {
            var flash = Props.muzzleFlash;
            if (flash == null) return;
            Mote_MuzzleFlash beam = (Mote_MuzzleFlash)ThingMaker.MakeThing(TiberiumDefOf.Mote_MuzzleFlash);
            Material mat = flash.Graphic.MatSingle;
            beam.Scale = flash.scale;
            beam.solidTimeOverride = flash.solidTime;
            beam.fadeInTimeOverride = flash.fadeInTime;
            beam.fadeOutTimeOverride = flash.fadeOutTime;
            beam.AttachMaterial(mat, Color.white);
            beam.SetLookDirection(origin, intendedTarget.CenterVector3);
            beam.Attach(caster);
            GenSpawn.Spawn(beam, caster.Position, caster.Map, WipeMode.Vanish);
        }

        private void RotateNextShotIndex()
        {
            lastOffsetIndex = offsetIndex;
            offsetIndex++;
            if (offsetIndex > (maxOffsetCount - 1))
                offsetIndex = 0;
        }

        public override void WarmupComplete()
        {
            burstShotsLeft = ShotsPerBurst;
            state = VerbState.Bursting;
            TryCastNextBurstShot();
            if (CasterIsPawn && currentTarget.HasThing)
            {
                if (currentTarget.Thing is Pawn pawn && pawn.IsColonistPlayerControlled)
                {
                    Find.BattleLog.Add(new BattleLogEntry_RangedFire(this.caster, this.currentTarget.HasThing ? this.currentTarget.Thing : null, (base.EquipmentSource != null) ? base.EquipmentSource.def : null, this.Projectile, this.ShotsPerBurst > 1));
                }
            }

            //var tibCost = Props.tiberiumCostPerBurst;
            //if(tibCost?.CanPayWith(TiberiumComp.Container)?? false)
            Props.tiberiumCostPerBurst?.DoPayWith(TiberiumComp);
        }

        protected override bool TryCastShot()
        {
            var flag = IsBeam ? TryCastBeam() : TryCastProjectile();

            if (flag)
                Notify_SingleShot();

            if (flag && Props.tiberiumCostPerShot != null)
            {
                if (Props.tiberiumCostPerShot.CanPayWith(TiberiumComp.TiberiumComp))
                    Props.tiberiumCostPerShot.DoPayWith(TiberiumComp);
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
            if (!base.Available()) return false;

            if (Props.powerConsumption > 0)
            {

            }

            if (Props.tiberiumCostPerBurst != null)
            {
                return Props.tiberiumCostPerBurst.CanPayWith(TiberiumComp.TiberiumComp);
            }

            if (Props.tiberiumCostPerShot != null)
            {
                return Props.tiberiumCostPerShot.CanPayWith(TiberiumComp.TiberiumComp);
            }

            if (CasterIsPawn)
            {
                Pawn casterPawn = CasterPawn;
                if (casterPawn.Faction != Faction.OfPlayer && casterPawn.mindState.MeleeThreatStillThreat &&
                    casterPawn.mindState.meleeThreat.Position.AdjacentTo8WayOrInside(casterPawn.Position))
                {
                    return false;
                }
            }

            return IsBeam || Projectile != null;
        }

        public void CastProjectile(IntVec3 origin, Thing caster, Vector3 drawPos, LocalTargetInfo usedTarget, LocalTargetInfo intendedTarget, ProjectileHitFlags flags, bool avoidFriendly, Thing equipmentSource, ThingDef targetCoverDef)
        {
            Projectile projectile = (Projectile)GenSpawn.Spawn(Projectile, origin, caster.Map, WipeMode.Vanish);
            projectile.Launch(caster, drawPos, usedTarget, intendedTarget, flags, avoidFriendly, equipmentSource, targetCoverDef);

            DoMuzzleFlash(drawPos, intendedTarget);
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
            Vector3 drawPos = ShotOrigin;
            //Projectile projectile2 = (Projectile)GenSpawn.Spawn(projectile, shootLine.Source, caster.Map, WipeMode.Vanish);
            if (verbProps.ForcedMissRadius > 0.5f)
            {
                float num = VerbUtility.CalculateAdjustedForcedMiss(verbProps.ForcedMissRadius, currentTarget.Cell - caster.Position);
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
                        CastProjectile(shootLine.Source, launcher, drawPos, c, currentTarget, projectileHitFlags, Props.avoidFriendlyFire, equipment, null);
                        //projectile2.Launch(launcher, drawPos, c, currentTarget, projectileHitFlags, Props.avoidFriendlyFire, equipment, null);
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

                CastProjectile(shootLine.Source, launcher, drawPos, shootLine.Dest, currentTarget, projectileHitFlags2, Props.avoidFriendlyFire, equipment, targetCoverDef);
                //projectile2.Launch(launcher, drawPos, shootLine.Dest, this.currentTarget, projectileHitFlags2, Props.avoidFriendlyFire, equipment, targetCoverDef);
                return true;
            }
            if (currentTarget.Thing != null && currentTarget.Thing.def.category == ThingCategory.Pawn && !Rand.Chance(shotReport.PassCoverChance))
            {
                ProjectileHitFlags projectileHitFlags3 = ProjectileHitFlags.NonTargetWorld;
                if (this.canHitNonTargetPawnsNow)
                {
                    projectileHitFlags3 |= ProjectileHitFlags.NonTargetPawns;
                }
                CastProjectile(shootLine.Source, launcher, drawPos, randomCoverToMissInto, currentTarget, projectileHitFlags3, Props.avoidFriendlyFire, equipment, targetCoverDef);
                //projectile2.Launch(launcher, drawPos, randomCoverToMissInto, this.currentTarget, projectileHitFlags3, Props.avoidFriendlyFire, equipment, targetCoverDef);
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
                CastProjectile(shootLine.Source, launcher, drawPos, currentTarget, currentTarget, projectileHitFlags4, Props.avoidFriendlyFire, equipment, targetCoverDef);
                //projectile2.Launch(launcher, drawPos, currentTarget, currentTarget, projectileHitFlags4, Props.avoidFriendlyFire, equipment, targetCoverDef);
            }
            else
            {
                CastProjectile(shootLine.Source, launcher, drawPos, shootLine.Dest, currentTarget, projectileHitFlags4, Props.avoidFriendlyFire, equipment, targetCoverDef);
                //projectile2.Launch(launcher, drawPos, shootLine.Dest, currentTarget, projectileHitFlags4, Props.avoidFriendlyFire, equipment, targetCoverDef);
            }
            return true;
        }

        public void SwitchProjectile()
        {
            if (Projectile == Props.defaultProjectile)
            {
                SetProjectile(Props.secondaryProjectile);
                return;
            }

            if (Projectile == Props.secondaryProjectile)
            {
                SetProjectile(Props.defaultProjectile);
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
            return false;
        }

        public LocalTargetInfo AdjustedTarget(LocalTargetInfo intended, ref ShootLine shootLine, out ProjectileHitFlags flags)
        {
            flags = ProjectileHitFlags.NonTargetWorld;
            if (verbProps.ForcedMissRadius > 0.5f)
            {
                float num = VerbUtility.CalculateAdjustedForcedMiss(verbProps.ForcedMissRadius, intended.Cell - caster.Position);
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

    public class MuzzleFlashProperties
    {
        private Graphic graphicInt;

        public GraphicData flashGraphicData;
        
        public float scale = 1;

        public float fadeInTime = 0f;
        public float solidTime = 0.25f;
        public float fadeOutTime = 0f;

        public Graphic Graphic => graphicInt ??= flashGraphicData.Graphic;
    }

    public class VerbProperties_TR : VerbProperties
    {
        //Information
        public string description;

        //Misc..
        public VerbBurstMode mode = VerbBurstMode.Normal;

        //Functional
        public bool isProjectile = true;
        public bool avoidFriendlyFire;
        public int shotIntervalTicks = 10;
        public ThingDef secondaryProjectile;

        public BeamProperties beamProps;

        public float powerConsumption = 0;
        public NetworkCost tiberiumCostPerBurst;
        public NetworkCost tiberiumCostPerShot;

        //
        public SoundDef chargeSound;

        //Graphical
        public MuzzleFlashProperties muzzleFlash;
        public List<Vector3> originOffsets;
    }
}
