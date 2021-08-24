using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace TiberiumRim
{
    public class TurretGun : IAttackTarget, IAttackTargetSearcher
    {
        //public Building_TRTurret parent;

        private ITurretHolder parentInt;
        public TurretProperties props;
        public TurretGunTop top;
        private int lastShotIndex = 0;
        private int curShotIndex = 0;
        private int lastAttackTargetTick;
        private int maxShotRotations = 1;
        private LocalTargetInfo lastAttackedTarget;
        private LocalTargetInfo forcedTarget = LocalTargetInfo.Invalid;
        private LocalTargetInfo currentTargetInt = LocalTargetInfo.Invalid;

        public int burstWarmupTicksLeft;
        public int burstCooldownTicksLeft;

        public LocalTargetInfo LastAttackedTarget => lastAttackedTarget;
        public LocalTargetInfo CurrentTarget => currentTargetInt;
        public LocalTargetInfo TargetCurrentlyAimingAt
        {
            get => CurrentTarget;
        }

        public Thing Thing => Parent;
        public Thing Parent => ParentHolder.Parent;
        public ITurretHolder ParentHolder => parentInt; //parent as ITurretHolder;
        protected CompRefuelable RefuelComp => ParentHolder.RefuelComp;
        protected CompPowerTrader PowerComp => ParentHolder.PowerComp;
        protected CompMannable MannableComp => ParentHolder.MannableComp;
        public CompEquippable GunCompEq => Gun.TryGetComp<CompEquippable>();
        public Thing Gun { get; private set; }

        public Verb_TR AttackVerb => (Verb_TR)GunCompEq.PrimaryVerb;
        public Verb CurrentEffectiveVerb => AttackVerb;
        public float TurretRotation => top.CurRotation;
        public int LastAttackTargetTick => lastAttackTargetTick;
        public int ShotIndex => curShotIndex;

        public bool Continuous => props.continuous;
        public bool HasTurret => props.turretTop != null;
        public bool IsMortar => AttackVerb.IsMortar;
        public bool NeedsRoof => IsMortar;
        private bool WarmingUp => burstWarmupTicksLeft > 0;

        private bool CanExtractShell
        {
            get
            {
                if (!ParentHolder.PlayerControlled)
                    return false;
                CompChangeableProjectile compChangeableProjectile = Gun.TryGetComp<CompChangeableProjectile>();
                return compChangeableProjectile != null && compChangeableProjectile.Loaded;
            }
        }

        public TurretGun()
        { }

        public void Setup(TurretProperties props, ITurretHolder parent)
        {
            this.props = props;
            this.parentInt = parent;
            Gun = ThingMaker.MakeThing(props.turretGunDef, null);
            UpdateGunVerbs();
            if (HasTurret)
            {
                top = new TurretGunTop(this);
                int max1 = 1,
                    max2 = 1;
                if (props.turretTop.barrels != null)
                    max1 = props.turretTop.barrels.Count;
                if (AttackVerb.Props.originOffsets != null)
                    max2 = AttackVerb.Props.originOffsets.Count;
                maxShotRotations = Math.Max(max1, max2);
            }
        }

        private void UpdateGunVerbs()
        {
            List<Verb> allVerbs = Gun.TryGetComp<CompEquippable>().AllVerbs;
            foreach (var verb in allVerbs)
            {
                verb.caster = ParentHolder.Caster;
                verb.castCompleteCallback = new Action(BurstComplete);
                if(verb is Verb_TR vt)
                {
                    vt.castingGun = this;
                }
            }
        }

        public void TurretTick(bool isReady)
        {
            if(HasTurret)
                top.BarrelTick();
            if (!isReady)
            {
                ResetCurrentTarget();
                return;
            }
            if (CanExtractShell && ParentHolder.MannedByColonist)
            {
                CompChangeableProjectile compChangeableProjectile = this.Gun.TryGetComp<CompChangeableProjectile>();
                if (!compChangeableProjectile.allowedShellsSettings.AllowedToAccept(compChangeableProjectile.LoadedShell))
                    ExtractShell();
            }

            if (forcedTarget.ThingDestroyed || (forcedTarget.IsValid && !ParentHolder.CanSetForcedTarget))
            {
                ResetForcedTarget();
            }
            GunCompEq.verbTracker.VerbsTick();
            if (!ParentHolder.Stunner.Stunned && AttackVerb.state != VerbState.Bursting)
            {
                if (Continuous)
                {
                    TryStartShootSomething(true);
                }
                else if (WarmingUp)
                {
                    burstWarmupTicksLeft--;
                    if (burstWarmupTicksLeft == 0)
                        BeginBurst();
                }
                else
                {
                    if (burstCooldownTicksLeft > 0)
                    {
                        burstCooldownTicksLeft--;
                    }
                    if (burstCooldownTicksLeft <= 0 && Parent.IsHashIntervalTick(AttackVerb.Props.shotIntervalTicks))
                    {
                        TryStartShootSomething(false);
                    }
                }
                top?.TurretTopTick();
            }
        }

        private void StartShooting()
        {
            if (Continuous)
            {
                //Continuous Shot

            }
            else
            {
                //Burst Shot

            }
        }

        protected void TryStartShootSomething(bool canBeginBurstImmediately)
        {
            if (!Parent.Spawned || (ParentHolder.HoldingFire && ParentHolder.CanToggleHoldFire) || NeedsRoof && Parent.Map.roofGrid.Roofed(Parent.Position) || !AttackVerb.Available())
            {
                ResetCurrentTarget();
                return;
            }

            currentTargetInt = forcedTarget.IsValid ? forcedTarget : TryFindNewTarget();

            if (CurrentTarget.IsValid)
            {
                if (!top?.OnTarget ?? false) return;
                if (canBeginBurstImmediately)
                {
                    BeginBurst();
                }
                else if (props.turretBurstWarmupTime > 0f)
                {
                    burstWarmupTicksLeft = props.turretBurstWarmupTime.SecondsToTicks();
                    //If charge sound available, play it
                    AttackVerb.Props.chargeSound?.PlayOneShot(SoundInfo.InMap(new TargetInfo(Parent), MaintenanceType.None));
                }
            }
            else
                ResetCurrentTarget();
        }

        protected LocalTargetInfo TryFindNewTarget()
        {
            IAttackTargetSearcher attackTargetSearcher = TargSearcher();
            Faction faction = attackTargetSearcher.Thing.Faction;
            float range = AttackVerb.verbProps.range;
            Building t;
            if (TRUtils.RandValue < 0.5f && NeedsRoof && faction.HostileTo(Faction.OfPlayer) && Parent.Map.listerBuildings.allBuildingsColonist.Where(delegate (Building x)
            {
                float num = AttackVerb.verbProps.EffectiveMinRange(x, Parent);
                float num2 = x.Position.DistanceToSquared(Parent.Position);
                return num2 > num * num && num2 < range * range;
            }).TryRandomElement(out t))
            {
                return t;
            }
            TargetScanFlags targetScanFlags = TargetScanFlags.NeedThreat;
            if (!NeedsRoof)
            {
                targetScanFlags |= TargetScanFlags.NeedLOSToAll;
                targetScanFlags |= TargetScanFlags.LOSBlockableByGas;
            }
            if (AttackVerb.IsIncendiary())
            {
                targetScanFlags |= TargetScanFlags.NeedNonBurning;
            }
            return (Thing)AttackTargetFinder.BestShootTargetFromCurrentPosition(attackTargetSearcher, targetScanFlags, new Predicate<Thing>(IsValidTarget), 0f, 9999f);
        }

        protected void BeginBurst()
        {
            AttackVerb.TryStartCastOn(CurrentTarget, false, true);
            OnAttackedTarget(CurrentTarget);
        }

        public void OrderAttack(LocalTargetInfo targ)
        {
            if (forcedTarget != targ)
            {
                forcedTarget = targ;
                if (burstCooldownTicksLeft <= 0)
                    TryStartShootSomething(false);
            }
        }

        private void ExtractShell()
        {
            GenPlace.TryPlaceThing(this.Gun.TryGetComp<CompChangeableProjectile>().RemoveShell(), Parent.Position, Parent.Map, ThingPlaceMode.Near, null, null);
        }

        public void ResetForcedTarget()
        {
            forcedTarget = LocalTargetInfo.Invalid;
            burstWarmupTicksLeft = 0;
            if (burstCooldownTicksLeft <= 0)
                TryStartShootSomething(false);
        }

        public void ResetCurrentTarget()
        {
            currentTargetInt = LocalTargetInfo.Invalid;
            burstWarmupTicksLeft = 0;
        }

        public void Notify_FiredSingleProjectile()
        {
            top?.Shoot(curShotIndex);
            RotateNextShotIndex();
            ParentHolder.Notify_ProjectileFired();
        }

        private void RotateNextShotIndex()
        {
            lastShotIndex = curShotIndex;
            curShotIndex++;
            if(curShotIndex > (maxShotRotations - 1))
                curShotIndex = 0;
        }

        private void OnAttackedTarget(LocalTargetInfo target)
        {
            lastAttackTargetTick = Find.TickManager.TicksGame;
            lastAttackedTarget = target;
        }

        private void BurstComplete()
        {
            burstCooldownTicksLeft = BurstCooldownTime().SecondsToTicks();
        }

        public float BurstCooldownTime()
        {
            if (props.turretBurstCooldownTime >= 0f)
            {
                return props.turretBurstCooldownTime;
            }
            return AttackVerb.verbProps.defaultCooldownTime;
        }

        private IAttackTargetSearcher TargSearcher()
        {
            if (MannableComp != null && MannableComp.MannedNow)
                return MannableComp.ManningPawn;
            return this;
        }

        private bool IsValidTarget(Thing t)
        {
            if (!(t is Pawn pawn)) return true;
            /*
            if(tiberium.burstMode == TurretBurstMode.ToTarget && tiberium.avoidFriendlyFire)
            {
                ShootLine line = new ShootLine(parent.Position, pawn.Position);
                if(line.Points().Any(P => P.GetFirstBuilding(parent.Map) is Building b && b != parent && b.Faction.IsPlayer))
                {
                    return false;
                }
            }
            */
            if (NeedsRoof)
            {
                RoofDef roofDef = Parent.Map.roofGrid.RoofAt(t.Position);
                if (roofDef != null && roofDef.isThickRoof)
                {
                    return false;
                }
            }
            if (MannableComp == null)
            {
                return !GenAI.MachinesLike(Parent.Faction, pawn);
            }
            /*
            if (ParentHolder.CurrentTarget != null && ParentHolder.CurrentTarget.Parent != t)
                return false;
            if(ParentHolder.HasTarget(t))
            */
            if (pawn.RaceProps.Animal && pawn.Faction == Faction.OfPlayer)
            {
                return false;
            }
            return true;
        }

        public bool ThreatDisabled(IAttackTargetSearcher disabledFor)
        {
            CompPowerTrader comp = PowerComp;
            if (comp != null && !comp.PowerOn)
            {
                return true;
            }
            CompMannable comp2 = MannableComp;
            return comp2 != null && !comp2.MannedNow;
        }

        public Graphic TurretGraphic => props.turretTop.turret.Graphic;
        public Vector3 DrawPos => Parent.DrawPos + props.drawOffset;

        public float TargetPriorityFactor => 1f;

        public void Draw()
        {
            if(HasTurret)
                top.DrawTurret();
            if (Find.Selector.IsSelected(Parent))
                DrawSelectionOverlays();
        }

        private void DrawSelectionOverlays()
        {
            if (forcedTarget.IsValid && (!forcedTarget.HasThing || forcedTarget.Thing.Spawned))
            {
                Vector3 b = forcedTarget.HasThing ? forcedTarget.Thing.TrueCenter() : forcedTarget.CenterVector3;
                Vector3 a = DrawPos;
                b.y = AltitudeLayer.MetaOverlays.AltitudeFor();
                a.y = b.y;
                GenDraw.DrawLineBetween(a, b, TiberiumContent.ForcedTargetLineMat);
            }
            float range = AttackVerb.verbProps.range;
            if (range < 90f)
            {
                GenDraw.DrawRadiusRing(Parent.Position, range);
            }
            float num = AttackVerb.verbProps.EffectiveMinRange(true);
            if (num < 90f && num > 0.1f)
            {
                GenDraw.DrawRadiusRing(Parent.Position, num);
            }

            if (HasTurret && WarmingUp)
            {
                int degreesWide = (int)((float)this.burstWarmupTicksLeft * 0.5f);
                GenDraw.DrawAimPieRaw(DrawPos + new Vector3(0f, top.props.barrelMuzzleOffset.magnitude, 0f), TurretRotation, degreesWide);
                //GenDraw.DrawAimPie(parent, this.CurrentTarget, degreesWide, (float)this.parent.props.size.x * 0.5f);
            }
        }

        public string GetUniqueLoadID()
        {
            return Parent.ThingID + "_TurretGun";
        }
    }

    public class TurretGunTop
    {
        public TurretGun parent;
        public TurretTopProperties props;
        public List<TurretBarrel> barrels = new List<TurretBarrel>();
        private float rotation;
        private float targetRot = 20;
        public float speed;
        private bool clockWise = true;
        private int ticksUntilTurn;
        private int turnTicks;
        private bool targetAcquired = false;

        public TurretGunTop(TurretGun parent)
        {
            this.parent = parent;
            props = parent.props.turretTop;
            if(props.barrels != null)
            {
                foreach(var barrel in props.barrels)
                {
                    barrels.Add(new TurretBarrel(this, barrel));
                }
            }
        }

        //Turret rotation inspired by Rimatomics
        public bool OnTarget
        {
            get
            {
                if (parent.CurrentTarget.IsValid)
                {
                    targetRot = (parent.CurrentTarget.CenterVector3 - parent.DrawPos).AngleFlat();
                    return Quaternion.Angle(rotation.ToQuat(), targetRot.ToQuat()) < props.aimAngle;
                }
                return false;
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

        public void Shoot(int index)
        {
            if (!barrels.NullOrEmpty() && barrels.Count > index)
            {
                barrels[index].Shoot();
            }
        }

        public void BarrelTick()
        {
            foreach(TurretBarrel barrel in barrels)
            {
                barrel.BarrelTick();
            }
        }

        public void TurretTopTick()
        {
            LocalTargetInfo currentTarget = this.parent.CurrentTarget;
            if (!currentTarget.IsValid)
            {
                if (targetAcquired)
                    targetAcquired = false;
            }
            if (currentTarget.IsValid)
            {
                targetRot = (parent.CurrentTarget.CenterVector3 - parent.DrawPos).AngleFlat();
                turnTicks = 0;
            }
            else if(ticksUntilTurn > 0)
            {
                ticksUntilTurn--;
                if(ticksUntilTurn == 0)
                {
                    clockWise = !(Rand.Value > 0.5);
                    turnTicks = TRUtils.Range(props.idleDuration);
                }
            }
            else 
            {
                targetRot += clockWise ? 0.26f : -0.26f;
                turnTicks--;
                if(turnTicks <= 0)
                    ticksUntilTurn = TRUtils.Range(props.idleInterval);
            }
            rotation = Mathf.SmoothDampAngle(rotation, targetRot, ref speed, 0.01f, props.speed, 0.01666f);
            if (OnTarget && !targetAcquired)
            {
                targetAcquired = true;
                SoundDefOf.TurretAcquireTarget.PlayOneShot(new TargetInfo(parent.Parent.Position, parent.Parent.Map, false));
            }
        }

        public Vector3 DrawPos => new Vector3(parent.DrawPos.x, AltitudeLayer.BuildingOnTop.AltitudeFor(), parent.DrawPos.z);

        public void DrawTurret()
        {
            TRUtils.Draw(parent.TurretGraphic, DrawPos, Rot4.North, CurRotation, null);
            barrels.ForEach(b => b.Draw());
        }
    }

    public class TurretBarrel
    {
        private TurretGunTop parent;
        private TurretBarrelProperties props;
        private float currentRecoil = 0;
        private float wantedRecoil = 0;
        public float currentVelocity = 0;
        private float speed = 100;


        private static float smoothTime = 0.01f;
        private static float deltaTime = 0.01666f;

        public TurretBarrel(TurretGunTop parent, TurretBarrelProperties props)
        {
            this.parent = parent;
            this.props = props;
        }

        public void Shoot()
        {
            wantedRecoil = 1;
            speed = parent.props.recoilSpeed;
        }

        public void BarrelTick()
        {
            currentRecoil = Mathf.SmoothDamp(currentRecoil, wantedRecoil, ref currentVelocity, smoothTime, speed, deltaTime);
            //Log.Message("Current Recoil: " + currentRecoil + " currentVelocity: " + currentVelocity + " curSpeed: " + speed, true);
            if(wantedRecoil > 0 && ((wantedRecoil - currentRecoil) <= 0.01))
            {
                wantedRecoil = 0;
                speed = parent.props.resetSpeed;
            }
        }

        [TweakValue("TurretGunTop_BarrelOffset", -5f, 5f)]
        private static float barrelOffset = 0f;

        public Graphic Graphic => props.graphic.Graphic;

        public Vector3 DrawPos
        {
            get
            {
                var drawPos = parent.DrawPos;
                var offset = props.barrelOffset + new Vector3(0,0, barrelOffset) + (props.recoilOffset * currentRecoil);
                drawPos += Quaternion.Euler(0, parent.CurRotation, 0) * offset;
                drawPos.y = AltitudeLayer.BuildingOnTop.AltitudeFor() + props.altitudeOffset;
                return drawPos;
            }
        }

        public void Draw()
        {
            //var mesh = graphic.MeshAt(Rot4.North);
            TRUtils.Draw(Graphic, DrawPos, Rot4.North, parent.CurRotation, null);
            //Graphics.DrawMesh(mesh, DrawPos, parent.CurRotation.ToQuat(), graphic.MatSingle, 0);
        }
    }
}
