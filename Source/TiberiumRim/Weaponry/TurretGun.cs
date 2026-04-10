using System;
using System.Collections.Generic;
using System.Linq;
using LudeonTK;
using RimWorld;
using TR.TextureContent;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace TR;

public class TurretGun : IAttackTarget, IAttackTargetSearcher
{
    public int burstCooldownTicksLeft;

    public int burstWarmupTicksLeft;
    private LocalTargetInfo forcedTarget = LocalTargetInfo.Invalid;
    private int lastShotIndex;

    private int maxShotRotations = 1;
    //public Building_TRTurret parent;

    public TurretProperties props;
    public TurretGunTop top;
    public LocalTargetInfo CurrentTarget { get; private set; } = LocalTargetInfo.Invalid;

    public Thing Parent => ParentHolder.Parent;
    public ITurretHolder ParentHolder { get; private set; }

    protected CompRefuelable RefuelComp => ParentHolder.RefuelComp;
    protected CompPowerTrader PowerComp => ParentHolder.PowerComp;
    protected CompMannable MannableComp => ParentHolder.MannableComp;
    public CompEquippable GunCompEq => Gun.TryGetComp<CompEquippable>();
    public Thing Gun { get; private set; }

    public Verb_TR AttackVerb => (Verb_TR)GunCompEq.PrimaryVerb;
    public float TurretRotation => top.CurRotation;
    public int ShotIndex { get; private set; }

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
            var compChangeableProjectile = Gun.TryGetComp<CompChangeableProjectile>();
            return compChangeableProjectile != null && compChangeableProjectile.Loaded;
        }
    }

    public Graphic TurretGraphic => props.turretTop.turret.Graphic;
    public Vector3 DrawPos => Parent.DrawPos + props.drawOffset;
    public LocalTargetInfo TargetCurrentlyAimingAt => CurrentTarget;

    public Thing Thing => Parent;

    public bool ThreatDisabled(IAttackTargetSearcher disabledFor)
    {
        var comp = PowerComp;
        if (comp != null && !comp.PowerOn) return true;
        var comp2 = MannableComp;
        return comp2 != null && !comp2.MannedNow;
    }

    public float TargetPriorityFactor => 1f;

    public string GetUniqueLoadID()
    {
        return Parent.ThingID + "_TurretGun";
    }

    public LocalTargetInfo LastAttackedTarget { get; private set; }

    public Verb CurrentEffectiveVerb => AttackVerb;
    public int LastAttackTargetTick { get; private set; }

    public void Setup(TurretProperties props, ITurretHolder parent)
    {
        this.props = props;
        ParentHolder = parent;
        Gun = ThingMaker.MakeThing(props.turretGunDef);
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
        var allVerbs = Gun.TryGetComp<CompEquippable>().AllVerbs;
        foreach (var verb in allVerbs)
        {
            verb.caster = ParentHolder.Caster;
            verb.castCompleteCallback = BurstComplete;
            if (verb is Verb_TR vt) vt.castingGun = this;
        }
    }

    public void TurretTick(bool isReady)
    {
        if (HasTurret)
            top.BarrelTick();
        if (!isReady)
        {
            ResetCurrentTarget();
            return;
        }

        if (CanExtractShell && ParentHolder.MannedByColonist)
        {
            var compChangeableProjectile = Gun.TryGetComp<CompChangeableProjectile>();
            if (!compChangeableProjectile.allowedShellsSettings.AllowedToAccept(compChangeableProjectile.LoadedShell))
                ExtractShell();
        }

        if (forcedTarget.ThingDestroyed || (forcedTarget.IsValid && !ParentHolder.CanSetForcedTarget))
            ResetForcedTarget();
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
                if (burstCooldownTicksLeft > 0) burstCooldownTicksLeft--;
                if (burstCooldownTicksLeft <= 0 && Parent.IsHashIntervalTick(AttackVerb.Props.shotIntervalTicks))
                    TryStartShootSomething(false);
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
        //Burst Shot
    }

    protected void TryStartShootSomething(bool canBeginBurstImmediately)
    {
        if (!Parent.Spawned || (ParentHolder.HoldingFire && ParentHolder.CanToggleHoldFire) ||
            (NeedsRoof && Parent.Map.roofGrid.Roofed(Parent.Position)) || !AttackVerb.Available())
        {
            ResetCurrentTarget();
            return;
        }

        CurrentTarget = forcedTarget.IsValid ? forcedTarget : TryFindNewTarget();

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
                AttackVerb.Props.chargeSound?.PlayOneShot(SoundInfo.InMap(new TargetInfo(Parent)));
            }
        }
        else
        {
            ResetCurrentTarget();
        }
    }

    protected LocalTargetInfo TryFindNewTarget()
    {
        var attackTargetSearcher = TargSearcher();
        var faction = attackTargetSearcher.Thing.Faction;
        var range = AttackVerb.verbProps.range;
        Building t;
        if (TRandom.RandValue < 0.5f && NeedsRoof && faction.HostileTo(Faction.OfPlayer) && Parent.Map.listerBuildings
                .allBuildingsColonist.Where(delegate(Building x)
                {
                    var num = AttackVerb.verbProps.EffectiveMinRange(x, Parent);
                    float num2 = x.Position.DistanceToSquared(Parent.Position);
                    return num2 > num * num && num2 < range * range;
                }).TryRandomElement(out t))
            return t;
        var targetScanFlags = TargetScanFlags.NeedThreat;
        if (!NeedsRoof)
        {
            targetScanFlags |= TargetScanFlags.NeedLOSToAll;
            targetScanFlags |= TargetScanFlags.LOSBlockableByGas;
        }

        if (AttackVerb.IsIncendiary_Ranged()) targetScanFlags |= TargetScanFlags.NeedNonBurning;
        return (Thing)AttackTargetFinder.BestShootTargetFromCurrentPosition(attackTargetSearcher, targetScanFlags,
            IsValidTarget);
    }

    protected void BeginBurst()
    {
        AttackVerb.TryStartCastOn(CurrentTarget);
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
        GenPlace.TryPlaceThing(Gun.TryGetComp<CompChangeableProjectile>().RemoveShell(), Parent.Position, Parent.Map,
            ThingPlaceMode.Near);
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
        CurrentTarget = LocalTargetInfo.Invalid;
        burstWarmupTicksLeft = 0;
    }

    public void Notify_FiredSingleProjectile()
    {
        top?.Shoot(ShotIndex);
        RotateNextShotIndex();
        ParentHolder.Notify_ProjectileFired();
    }

    private void RotateNextShotIndex()
    {
        lastShotIndex = ShotIndex;
        ShotIndex++;
        if (ShotIndex > maxShotRotations - 1)
            ShotIndex = 0;
    }

    private void OnAttackedTarget(LocalTargetInfo target)
    {
        LastAttackTargetTick = Find.TickManager.TicksGame;
        LastAttackedTarget = target;
    }

    private void BurstComplete()
    {
        burstCooldownTicksLeft = BurstCooldownTime().SecondsToTicks();
    }

    public float BurstCooldownTime()
    {
        if (props.turretBurstCooldownTime >= 0f) return props.turretBurstCooldownTime;
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
            var roofDef = Parent.Map.roofGrid.RoofAt(t.Position);
            if (roofDef != null && roofDef.isThickRoof) return false;
        }

        if (MannableComp == null) return !GenAI.MachinesLike(Parent.Faction, pawn);
        /*
        if (ParentHolder.CurrentTarget != null && ParentHolder.CurrentTarget.Parent != t)
            return false;
        if(ParentHolder.HasTarget(t))
        */
        if (pawn.RaceProps.Animal && pawn.Faction == Faction.OfPlayer) return false;
        return true;
    }

    public void Draw()
    {
        if (HasTurret)
            top.DrawTurret();
        if (Find.Selector.IsSelected(Parent))
            DrawSelectionOverlays();
    }

    private void DrawSelectionOverlays()
    {
        if (forcedTarget.IsValid && (!forcedTarget.HasThing || forcedTarget.Thing.Spawned))
        {
            var b = forcedTarget.HasThing ? forcedTarget.Thing.TrueCenter() : forcedTarget.CenterVector3;
            var a = DrawPos;
            b.y = AltitudeLayer.MetaOverlays.AltitudeFor();
            a.y = b.y;
            GenDraw.DrawLineBetween(a, b, TiberiumContent.ForcedTargetLineMat);
        }

        var range = AttackVerb.verbProps.range;
        if (range < 90f) GenDraw.DrawRadiusRing(Parent.Position, range);
        var num = AttackVerb.verbProps.EffectiveMinRange(true);
        if (num < 90f && num > 0.1f) GenDraw.DrawRadiusRing(Parent.Position, num);

        if (HasTurret && WarmingUp)
        {
            var degreesWide = (int)(burstWarmupTicksLeft * 0.5f);
            GenDraw.DrawAimPieRaw(DrawPos + new Vector3(0f, top.props.barrelMuzzleOffset.magnitude, 0f), TurretRotation,
                degreesWide);
            //GenDraw.DrawAimPie(parent, this.CurrentTarget, degreesWide, (float)this.parent.def.size.x * 0.5f);
        }
    }
}

public class TurretGunTop
{
    public List<TurretBarrel> barrels = new();
    private bool clockWise = true;
    public TurretGun parent;
    public TurretTopProperties props;
    private float rotation;
    public float speed;
    private bool targetAcquired;
    private float targetRot = 20;
    private int ticksUntilTurn;
    private int turnTicks;

    public TurretGunTop(TurretGun parent)
    {
        this.parent = parent;
        props = parent.props.turretTop;
        if (props.barrels != null)
            foreach (var barrel in props.barrels)
                barrels.Add(new TurretBarrel(this, barrel));
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
            if (value > 360) rotation = value - 360;
            if (value < 0) rotation = value + 360;
            rotation = value;
        }
    }

    public Vector3 DrawPos => new(parent.DrawPos.x, AltitudeLayer.BuildingOnTop.AltitudeFor(), parent.DrawPos.z);

    public void Shoot(int index)
    {
        if (!barrels.NullOrEmpty() && barrels.Count > index) barrels[index].Shoot();
    }

    public void BarrelTick()
    {
        foreach (var barrel in barrels) barrel.BarrelTick();
    }

    public void TurretTopTick()
    {
        var currentTarget = parent.CurrentTarget;
        if (!currentTarget.IsValid)
            if (targetAcquired)
                targetAcquired = false;
        if (currentTarget.IsValid)
        {
            targetRot = (parent.CurrentTarget.CenterVector3 - parent.DrawPos).AngleFlat();
            turnTicks = 0;
        }
        else if (ticksUntilTurn > 0)
        {
            ticksUntilTurn--;
            if (ticksUntilTurn == 0)
            {
                clockWise = !(Rand.Value > 0.5);
                turnTicks = TRUtils.Range(props.idleDuration);
            }
        }
        else
        {
            targetRot += clockWise ? 0.26f : -0.26f;
            turnTicks--;
            if (turnTicks <= 0)
                ticksUntilTurn = TRUtils.Range(props.idleInterval);
        }

        rotation = Mathf.SmoothDampAngle(rotation, targetRot, ref speed, 0.01f, props.speed, 0.01666f);
        if (OnTarget && !targetAcquired)
        {
            targetAcquired = true;
            SoundDefOf.TurretAcquireTarget.PlayOneShot(new TargetInfo(parent.Parent.Position, parent.Parent.Map));
        }
    }

    public void DrawTurret()
    {
        TRUtils.Draw(parent.TurretGraphic, DrawPos, Rot4.North, CurRotation, null);
        barrels.ForEach(b => b.Draw());
    }
}

public class TurretBarrel
{
    private static readonly float smoothTime = 0.01f;
    private static readonly float deltaTime = 0.01666f;

    [TweakValue("TurretGunTop_BarrelOffset", -5f, 5f)]
    private static readonly float barrelOffset = 0f;

    private readonly Graphic graphic;
    private readonly TurretGunTop parent;
    private readonly TurretBarrelProperties props;
    private float currentRecoil;
    public float currentVelocity;
    private float speed = 100;
    private float wantedRecoil;

    public TurretBarrel(TurretGunTop parent, TurretBarrelProperties props)
    {
        this.parent = parent;
        this.props = props;
        graphic = props.graphic.Graphic;
    }

    public Vector3 DrawPos
    {
        get
        {
            var drawPos = parent.DrawPos;
            var offset = props.barrelOffset + new Vector3(0, 0, barrelOffset) + props.recoilOffset * currentRecoil;
            drawPos += Quaternion.Euler(0, parent.CurRotation, 0) * offset;
            drawPos.y = AltitudeLayer.BuildingOnTop.AltitudeFor() + props.altitudeOffset;
            return drawPos;
        }
    }

    public void Shoot()
    {
        wantedRecoil = 1;
        speed = parent.props.recoilSpeed;
    }

    public void BarrelTick()
    {
        currentRecoil =
            Mathf.SmoothDamp(currentRecoil, wantedRecoil, ref currentVelocity, smoothTime, speed, deltaTime);
        //Log.Message("Current Recoil: " + currentRecoil + " currentVelocity: " + currentVelocity + " curSpeed: " + speed, true);
        if (wantedRecoil > 0 && wantedRecoil - currentRecoil <= 0.01)
        {
            wantedRecoil = 0;
            speed = parent.props.resetSpeed;
        }
    }

    public Graphic Graphic => props.graphic.Graphic;


    public void Draw()
    {
        //var mesh = graphic.MeshAt(Rot4.North);
        TRUtils.Draw(Graphic, DrawPos, Rot4.North, parent.CurRotation, null);
        //Graphics.DrawMesh(mesh, DrawPos, parent.CurRotation.ToQuat(), graphic.MatSingle, 0);
    }
}