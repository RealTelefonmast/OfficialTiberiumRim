using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;
using Verse.AI;
using Verse.Sound;

namespace TiberiumRim
{
    public class TurretGun : IAttackTarget, IAttackTargetSearcher
    {
        public static Material ForcedTargetLineMat = MaterialPool.MatFrom(GenDraw.LineTexPath, ShaderDatabase.Transparent, new Color(1f, 0.5f, 0.5f));
        public ThingWithComps parent;
        private CompPowerTrader powerComp;
        private CompMannable mannableComp;
        private Thing gun;

        public TurretProperties props;
        private TurretGunTop top;
        private int lastAttackTargetTick;
        private LocalTargetInfo lastAttackedTarget;
        private LocalTargetInfo forcedTarget = LocalTargetInfo.Invalid;
        private LocalTargetInfo currentTargetInt = LocalTargetInfo.Invalid;

        public int burstWarmupTicksLeft;
        public int burstCooldownTicksLeft;

        public ITurretHolder ParentHolder => parent as ITurretHolder;

        public TurretGun(TurretProperties props, ThingWithComps parent)
        {
            this.props = props;
            this.parent = parent;
        }

        public void Setup()
        {
            if(props.hasTurret)
                top = new TurretGunTop(this);

            powerComp = parent.GetComp<CompPowerTrader>();
            mannableComp = parent.GetComp<CompMannable>();
            gun = ThingMaker.MakeThing(props.turretGunDef, null);
            UpdateGunVerbs();
        }

        private void UpdateGunVerbs()
        {
            List<Verb> allVerbs = gun.TryGetComp<CompEquippable>().AllVerbs;
            for (int i = 0; i < allVerbs.Count; i++)
            {
                Verb verb = allVerbs[i];
                verb.caster = parent;
                verb.castCompleteCallback = new Action(BurstComplete);
                if(verb is Verb_TR vt)
                {
                    vt.castingGun = this;
                }
            }
        }

        public void TurretTick()
        {
            /*
            if (this.CanExtractShell && this.MannedByColonist)
            {
                CompChangeableProjectile compChangeableProjectile = this.gun.TryGetComp<CompChangeableProjectile>();
                if (!compChangeableProjectile.allowedShellsSettings.AllowedToAccept(compChangeableProjectile.LoadedShell))
                {
                    this.ExtractShell();
                }
            }
            */
            if (forcedTarget.IsValid && !ParentHolder.CanSetForcedTarget)
            {
                ResetForcedTarget();
            }
            if (this.forcedTarget.ThingDestroyed)
            {
                this.ResetForcedTarget();
            }
            bool flag = (this.powerComp == null || this.powerComp.PowerOn) && (this.mannableComp == null || this.mannableComp.MannedNow);
            if (flag && parent.Spawned)
            {
                this.GunCompEq.verbTracker.VerbsTick();
                if (this.AttackVerb.state != VerbState.Bursting)
                {
                    if (this.WarmingUp)
                    {
                        this.burstWarmupTicksLeft--;
                        if (this.burstWarmupTicksLeft == 0)
                        {
                            BeginBurst();
                        }
                    }
                    else
                    {
                        if (this.burstCooldownTicksLeft > 0)
                        {
                            this.burstCooldownTicksLeft--;
                        }
                        if (this.burstCooldownTicksLeft <= 0 && parent.IsHashIntervalTick(10))
                        {
                            this.TryStartShootSomething(true);
                        }
                    }
                    top?.TurretTopTick();
                }
            }
            else
            {
                this.ResetCurrentTarget();
            }
        }

        protected void TryStartShootSomething(bool canBeginBurstImmediately)
        {
            if (!parent.Spawned || (ParentHolder.HoldingFire && ParentHolder.CanToggleHoldFire) || (this.AttackVerb.ProjectileFliesOverhead() && parent.Map.roofGrid.Roofed(parent.Position)) || !this.AttackVerb.Available())
            {
                this.ResetCurrentTarget();
                return;
            }
            bool isValid = this.currentTargetInt.IsValid;
            if (this.forcedTarget.IsValid)
            {
                this.currentTargetInt = this.forcedTarget;
            }
            else
            {
                this.currentTargetInt = this.TryFindNewTarget();
                ParentHolder.AddTarget(currentTargetInt);
            }
            if (this.currentTargetInt.IsValid && (top?.OnTarget ?? true))
            {
                SoundDefOf.TurretAcquireTarget.PlayOneShot(new TargetInfo(parent.Position, parent.Map, false));
                if (props.turretBurstWarmupTime > 0f)
                {
                    this.burstWarmupTicksLeft = props.turretBurstWarmupTime.SecondsToTicks();
                }
                else if (canBeginBurstImmediately)
                {
                    this.BeginBurst();
                }
                else
                {
                    this.burstWarmupTicksLeft = 1;
                }
            }
            else
            {
                this.ResetCurrentTarget();
            }
        }

        protected void BeginBurst()
        {
            if (props.burstMode == TurretBurstMode.Normal)
            {
                this.AttackVerb.TryStartCastOn(this.CurrentTarget, false, true);
                OnAttackedTarget(this.CurrentTarget);
            }
            else if (props.burstMode == TurretBurstMode.ToTarget)
            {
                DoLineToTarget();
            }
        }

        private void DoLineToTarget()
        {
            var from = DrawPos.ToIntVec3();
            var to = CurrentTarget.Cell;
            var distance = from.DistanceTo(to);
            if(distance < props.burstToRange)
            {
                var normed = (to - from).ToVector3().normalized;
                normed *= props.burstToRange;
                IntVec3 newTo = from + normed.ToIntVec3();
                to = newTo;
            }
            var line = new ShootLine(from, to);
            foreach (IntVec3 cell in line.Points())
            {
                AttackVerb.TryStartCastOn(cell, false);
            }
            OnAttackedTarget(CurrentTarget);
        }

        public void OrderAttack(LocalTargetInfo targ)
        {
            if (!targ.IsValid)
            {
                if (this.forcedTarget.IsValid)
                {
                    this.ResetForcedTarget();
                }
                return;
            }
            if ((targ.Cell - parent.Position).LengthHorizontal < this.AttackVerb.verbProps.EffectiveMinRange(targ, parent))
            {
                Messages.Message("MessageTargetBelowMinimumRange".Translate(), parent, MessageTypeDefOf.RejectInput, false);
                return;
            }
            if ((targ.Cell - parent.Position).LengthHorizontal > this.AttackVerb.verbProps.range)
            {
                Messages.Message("MessageTargetBeyondMaximumRange".Translate(), parent, MessageTypeDefOf.RejectInput, false);
                return;
            }
            if (this.forcedTarget != targ)
            {
                this.forcedTarget = targ;
                if (this.burstCooldownTicksLeft <= 0)
                {
                    this.TryStartShootSomething(false);
                }
            }
            if (ParentHolder.HoldingFire)
            {
                Messages.Message("MessageTurretWontFireBecauseHoldFire".Translate(parent.def.label), parent, MessageTypeDefOf.RejectInput, false);
            }
        }

        public void ResetForcedTarget()
        {
            this.forcedTarget = LocalTargetInfo.Invalid;
            this.burstWarmupTicksLeft = 0;
            if (this.burstCooldownTicksLeft <= 0)
            {
                this.TryStartShootSomething(false);
            }
        }

        public void SetForcedTarget(LocalTargetInfo target)
        {
            forcedTarget = target;
        }

        public void ResetCurrentTarget()
        {
            this.currentTargetInt = LocalTargetInfo.Invalid;
            this.burstWarmupTicksLeft = 0;
        }

        private void OnAttackedTarget(LocalTargetInfo target)
        {
            this.lastAttackTargetTick = Find.TickManager.TicksGame;
            this.lastAttackedTarget = target;
        }

        private void BurstComplete()
        {
            this.burstCooldownTicksLeft = this.BurstCooldownTime().SecondsToTicks();
        }

        private float BurstCooldownTime()
        {
            if (props.turretBurstCooldownTime >= 0f)
            {
                return props.turretBurstCooldownTime;
            }
            return AttackVerb.verbProps.defaultCooldownTime;
        }

        protected LocalTargetInfo TryFindNewTarget()
        {
            IAttackTargetSearcher attackTargetSearcher = this.TargSearcher();
            Faction faction = attackTargetSearcher.Thing.Faction;
            float range = this.AttackVerb.verbProps.range;
            Building t;
            if (Rand.Value < 0.5f && this.AttackVerb.ProjectileFliesOverhead() && faction.HostileTo(Faction.OfPlayer) && parent.Map.listerBuildings.allBuildingsColonist.Where(delegate (Building x)
            {
                float num = this.AttackVerb.verbProps.EffectiveMinRange(x, parent);
                float num2 = (float)x.Position.DistanceToSquared(parent.Position);
                return num2 > num * num && num2 < range * range;
            }).TryRandomElement(out t))
            {
                return t;
            }
            TargetScanFlags targetScanFlags = TargetScanFlags.NeedThreat;
            if (!this.AttackVerb.ProjectileFliesOverhead())
            {
                targetScanFlags |= TargetScanFlags.NeedLOSToAll;
                targetScanFlags |= TargetScanFlags.LOSBlockableByGas;
            }
            if (this.AttackVerb.IsIncendiary())
            {
                targetScanFlags |= TargetScanFlags.NeedNonBurning;
            }
            return (Thing)AttackTargetFinder.BestShootTargetFromCurrentPosition(attackTargetSearcher, targetScanFlags, new Predicate<Thing>(this.IsValidTarget), 0f, 9999f);
        }

        private IAttackTargetSearcher TargSearcher()
        {
            if (this.mannableComp != null && this.mannableComp.MannedNow)
            {
                return this.mannableComp.ManningPawn;
            }
            return this;
        }

        private bool IsValidTarget(Thing t)
        {
            Pawn pawn = t as Pawn;
            if (pawn != null)
            {
                if (this.AttackVerb.ProjectileFliesOverhead())
                {
                    RoofDef roofDef = parent.Map.roofGrid.RoofAt(t.Position);
                    if (roofDef != null && roofDef.isThickRoof)
                    {
                        return false;
                    }
                }
                if (this.mannableComp == null)
                {
                    return !GenAI.MachinesLike(parent.Faction, pawn);
                }
                if (ParentHolder.FocusedTarget != null && ParentHolder.FocusedTarget.Thing != t)
                    return false;
                if(ParentHolder.HasTarget(t))
                if (pawn.RaceProps.Animal && pawn.Faction == Faction.OfPlayer)
                {
                    return false;
                }
            }
            return true;
        }

        public CompEquippable GunCompEq => gun.TryGetComp<CompEquippable>();
        public Thing Thing => parent;
        public LocalTargetInfo LastAttackedTarget => lastAttackedTarget;
        public LocalTargetInfo CurrentTarget => currentTargetInt;
        public LocalTargetInfo TargetCurrentlyAimingAt
        {
            get => forcedTarget;
            set => forcedTarget = value;
        }

        public Verb AttackVerb => GunCompEq.PrimaryVerb;
        public Verb CurrentEffectiveVerb => this.AttackVerb;
        public float TurretRotation => top.CurRotation;
        public int LastAttackTargetTick => this.lastAttackTargetTick;
        private bool WarmingUp => this.burstWarmupTicksLeft > 0;

        public Graphic Graphic
        {
            get
            {
                return props.graphic.Graphic;
            }
        }
        public Vector3 DrawPos
        {
            get
            {
                return parent.DrawPos + props.drawOffset;
            }
        }

        public void Draw()
        {
            if(props.hasTurret)
                top.DrawTurret();
            if (Find.Selector.IsSelected(parent))
                DrawSelectionOverlays();
        }

        private void DrawSelectionOverlays()
        {
            float range = this.AttackVerb.verbProps.range;
            if (range < 90f)
            {
                GenDraw.DrawRadiusRing(parent.Position, range);
            }
            float num = this.AttackVerb.verbProps.EffectiveMinRange(true);
            if (num < 90f && num > 0.1f)
            {
                GenDraw.DrawRadiusRing(parent.Position, num);
            }
            if (this.WarmingUp)
            {
                int degreesWide = (int)((float)this.burstWarmupTicksLeft * 0.5f);
                GenDraw.DrawAimPieRaw(DrawPos + new Vector3(0f, props.barrelOffset.magnitude, 0f), TurretRotation, degreesWide);
                //GenDraw.DrawAimPie(parent, this.CurrentTarget, degreesWide, (float)this.parent.def.size.x * 0.5f);
            }
            if (this.forcedTarget.IsValid && (!this.forcedTarget.HasThing || this.forcedTarget.Thing.Spawned))
            {
                Vector3 b;
                if (this.forcedTarget.HasThing)
                {
                    b = this.forcedTarget.Thing.TrueCenter();
                }
                else
                {
                    b = this.forcedTarget.Cell.ToVector3Shifted();
                }
                Vector3 a = DrawPos;
                b.y = AltitudeLayer.MetaOverlays.AltitudeFor();
                a.y = b.y;
                GenDraw.DrawLineBetween(a, b, Building_TurretGun.ForcedTargetLineMat);
            }
        }

        public string GetUniqueLoadID()
        {
            return parent.ThingID + "_TurretGun";
        }

        public bool ThreatDisabled(IAttackTargetSearcher disabledFor)
        {
            CompPowerTrader comp = parent.GetComp<CompPowerTrader>();
            if (comp != null && !comp.PowerOn)
            {
                return true;
            }
            CompMannable comp2 = parent.GetComp<CompMannable>();
            return comp2 != null && !comp2.MannedNow;
        }
    }

    public class TurretGunTop
    {
        public TurretGun parent;
        private float rotation;
        private float targetRot = 20;
        private float speed;
        private bool clockWise = true;
        private int ticksUntilTurn;
        private int turnTicks;

        public TurretGunTop(TurretGun parent)
        {
            this.parent = parent;
        }

        //Turret rotation inspired by Rimatomics
        public bool OnTarget
        {
            get
            {
                if (parent.CurrentTarget.IsValid)
                {
                    targetRot = (parent.CurrentTarget.CenterVector3 - parent.DrawPos).AngleFlat();
                }
                return Quaternion.Angle(rotation.ToQuat(), targetRot.ToQuat()) < parent.props.aimAngle;
            }
        }

        public float CurRotation
        {
            get => rotation;
            set
            {
                if (value > 360)
                {
                    rotation = value - 360;
                }
                if (value < 0)
                {
                    rotation = value + 360;
                }
                rotation = value;
            }
        }

        public void TurretTopTick()
        {
            LocalTargetInfo currentTarget = this.parent.CurrentTarget;
            if (currentTarget.IsValid)
            {
                targetRot = (parent.CurrentTarget.CenterVector3 - parent.DrawPos).AngleFlat();
                turnTicks = 0;
                /*
                float curRotation = (currentTarget.Cell.ToVector3Shifted() - this.parent.DrawPos).AngleFlat();
                this.CurRotation = curRotation;
                this.ticksUntilTurn = Rand.RangeInclusive(150, 350);
                */
            }
            else if(ticksUntilTurn > 0)
            {
                ticksUntilTurn--;
                if(ticksUntilTurn == 0)
                {
                    clockWise = Rand.Value > 0.5 ? false : true;
                    turnTicks = TRUtils.Range(parent.props.idleDuration);
                }
            }
            else 
            {
                targetRot += clockWise ? 0.26f : -0.26f;
                turnTicks--;
                if(turnTicks <= 0)
                    ticksUntilTurn = TRUtils.Range(parent.props.idleInterval);
            }
            rotation = Mathf.SmoothDampAngle(rotation, targetRot, ref speed, 0.01f, parent.props.speed, 0.01666f);
        }

        public void DrawTurret()
        {
            GraphicDrawInfo info = new GraphicDrawInfo(parent.Graphic, parent.DrawPos, parent.parent.Rotation, null, null);
            var drawPos = new Vector3(info.drawPos.x, AltitudeLayer.BuildingOnTop.AltitudeFor(), info.drawPos.z);
            Graphics.DrawMesh(info.drawMesh, drawPos, CurRotation.ToQuat(), info.drawMat, 0);
        }
    }
}
