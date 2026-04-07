using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace TeleCore.Systems.Turrets;

public class TurretGun : IAttackTargetSearcher
{
    private TurretProperties _props;
    private int _turretIndex;
    private Effecter? _progressBar;
    private Thing? _gun;

    //Turret Data
    private TurretGunTop _top;
    private int maxShotRotations;
    private int _curShotIndex;
    private int _lastShotIndex;

    //Local state
    private bool _holdFire;
    private bool _burstActivated;
    private int _burstWarmupTicksLeft;
    private int _burstCooldownTicksLeft;

    private LocalTargetInfo _forcedTarget = LocalTargetInfo.Invalid;
    private LocalTargetInfo _currentTargetInt;

    private int _lastAttackTargetTick;
    private LocalTargetInfo _lastAttackedTarget;

    public int BurstCoolDownTicksLeft => _burstCooldownTicksLeft;
    public bool HoldFire => _holdFire; //Also considers parent override
    public LocalTargetInfo ForcedTarget => _forcedTarget;

    //Local Turret Data
    public TurretProperties Props => _props;
    public Thing Gun => _gun;
    public TurretGunTop Top => _top;
    public CompEquippable GunCompEq => _gun.TryGetComp<CompEquippable>();

    public Vector3 DrawPos => ParentThing.DrawPos + _props.turretOffset;
    public float TurretRotation => _top?.CurRotation ?? 0;
    public int ShotIndex => _curShotIndex;

    public bool UsesTurretGunTop => _props.turretTop != null;
    public bool NeedsRoofless => IsMortar;
    public bool CanToggleHoldFire => Parent.PlayerControlled;
    public bool CanSetForcedTarget => (Parent.MannableComp != null || _props.canForceTarget) && Parent.PlayerControlled;
    private bool CanExtractShell
    {
        get
        {
            if (!Parent.PlayerControlled)
                return false;
            var compChangeableProjectile = Gun.TryGetComp<CompChangeableProjectile>();
            return compChangeableProjectile is { Loaded: true };
        }
    }

    //Verb
    public Verb AttackVerb => GunCompEq.PrimaryVerb;
    public Verb_Tele? TeleVerb => AttackVerb as Verb_Tele;
    public VerbProperties_Extended? VerbPropsExtended => TeleVerb?.Props;

    //State
    public LocalTargetInfo CurrentTarget => _currentTargetInt.IsValid ? _currentTargetInt : _forcedTarget;
    public bool WarmingUp => _burstWarmupTicksLeft > 0;
    public bool IsStunned => Parent.IsStunned; //TODO: Individual turrets can be stunned

    public float BurstCooldownTime
    {
        get
        {
            if (_props.turretBurstCooldownTime >= 0f) return _props.turretBurstCooldownTime;
            return AttackVerb.verbProps.defaultCooldownTime;
        }
    }

    //IAttackTargetSearcher
    public Thing Thing => ParentThing;
    public Verb CurrentEffectiveVerb => AttackVerb;
    public LocalTargetInfo LastAttackedTarget => _lastAttackedTarget;
    public int LastAttackTargetTick => _lastAttackTargetTick;

    #region Parent Data

    public ITurretHolder Parent { get; private set; }
    public Thing ParentThing => Parent.HolderThing;
    private bool IsMortar => ParentThing.def.building.IsMortar || AttackVerb is Verb_Tele { IsMortar: true } || AttackVerb.ProjectileFliesOverhead();

    #endregion

    #region Ticking and Updating

    public void TickGun()
    {
        if (CanExtractShell && Parent.MannedByColonist)
        {
            var compChangeableProjectile = _gun.TryGetComp<CompChangeableProjectile>();
            if (!compChangeableProjectile.allowedShellsSettings.AllowedToAccept(compChangeableProjectile.LoadedShell))
                ExtractShell();
        }

        if (_forcedTarget.IsValid && !CanSetForcedTarget)
            ResetForcedTarget();

        //Hnadled by parent
        // if (!this.CanToggleHoldFire)
        // {
        //     this.holdFire = false;
        // }

        if (_forcedTarget.ThingDestroyed)
            ResetForcedTarget();

        var mannedIfRequired = Parent.MannableComp == null || Parent.MannableComp.MannedNow;
        if (Parent.Active && mannedIfRequired && Parent is { IsStunned: false, Spawned: true })
        {
            _top?.TurretTopTick();
            GunCompEq.verbTracker.VerbsTick();
            if (AttackVerb.state != VerbState.Bursting)
            {
                _burstActivated = false;
                if (WarmingUp)
                {
                    _burstWarmupTicksLeft--;
                    if (_burstWarmupTicksLeft == 0)
                        BeginBurst();
                }
                else
                {
                    if (_burstCooldownTicksLeft > 0)
                    {
                        _burstCooldownTicksLeft--;
                        if (IsMortar)
                        {
                            if (_progressBar == null) _progressBar = EffecterDefOf.ProgressBar.Spawn();
                            _progressBar.EffectTick(ParentThing, TargetInfo.Invalid);
                            var mote = ((SubEffecter_ProgressBar)_progressBar.children[0]).mote;
                            mote.progress = 1f - Math.Max(_burstCooldownTicksLeft, 0) / (float)BurstCooldownTime.SecondsToTicks();
                            mote.offsetZ = -0.8f;
                        }
                    }

                    if (_burstCooldownTicksLeft <= 0 && ParentThing.IsHashIntervalTick(VerbPropsExtended?.shotIntervalTicks ?? 10))
                        TryStartShootSomething(true);
                }
            }
        }
        else
        {
            ResetCurrentTarget();
        }
    }

    private void BeginBurst()
    {
        AttackVerb.TryStartCastOn(CurrentTarget);
        OnAttackedTarget(CurrentTarget);
    }

    private void TryStartShootSomething(bool canBeginBurstImmediately)
    {
        if (_progressBar != null)
        {
            _progressBar.Cleanup();
            _progressBar = null;
        }

        if (!ParentThing.Spawned || (HoldFire && CanToggleHoldFire) || (NeedsRoofless && !ParentThing.Map.roofGrid.Roofed(ParentThing.Position)) || !AttackVerb.Available())
        {
            ResetCurrentTarget();
            return;
        }

        _currentTargetInt = _forcedTarget.IsValid ? _forcedTarget : TryFindNewTarget();

        if (CurrentTarget.IsValid)
        {
            StartTargeting(_currentTargetInt);

            if (!_top?.OnTarget ?? false) return;
            if (canBeginBurstImmediately)
                BeginBurst();
            else if (_props.turretBurstWarmupTime > 0f)
                _burstWarmupTicksLeft = _props.turretBurstWarmupTime.SecondsToTicks();
        }
        else
        {
            ResetCurrentTarget();
        }
    }

    public void Draw(Vector3 drawLoc)
    {
        //drawLoc += _props.turretOffset;
        if (UsesTurretGunTop)
            _top.DrawTurret(DrawPos);
        if (Find.Selector.IsSelected(ParentThing))
            DrawSelectionOverlays(DrawPos);
    }

    public void DrawSelectionOverlays(Vector3 drawLoc)
    {
        var range = AttackVerb.verbProps.range;
        if (range < 90f) GenDraw.DrawRadiusRing(ParentThing.Position, range);
        var num = AttackVerb.verbProps.EffectiveMinRange(true);
        if (num is < 90f and > 0.1f) GenDraw.DrawRadiusRing(ParentThing.Position, num);

        if (WarmingUp)
        {
            var degreesWide = (int)(_burstWarmupTicksLeft * 0.5f);
            var rot = _top?.CurRotation ?? (CurrentTarget.Thing.DrawPos - DrawPos).AngleFlat();
            GenDraw.DrawAimPieRaw(drawLoc + new Vector3(0f, ParentThing.def.size.x * 0.5f, 0f), rot, degreesWide);
        }

        if (_forcedTarget.IsValid && (!_forcedTarget.HasThing || _forcedTarget.Thing.Spawned))
        {
            Vector3 vector;
            if (_forcedTarget.HasThing)
            {
                vector = _forcedTarget.Thing.TrueCenter();
            }
            else
            {
                vector = _forcedTarget.Cell.ToVector3Shifted();
            }
            var a = ParentThing.TrueCenter();
            vector.y = AltitudeLayer.MetaOverlays.AltitudeFor();
            a.y = vector.y;
            GenDraw.DrawLineBetween(a, vector, Building_TurretGun.ForcedTargetLineMat);
        }
    }

    #endregion

    #region Turret Gun Rotation (Barrels)

    public void Notify_FiredSingleProjectile()
    {
        _top?.Notify_TurretShot(ShotIndex);

        _lastShotIndex = ShotIndex;
        _curShotIndex++;
        if (ShotIndex > maxShotRotations - 1)
            _curShotIndex = 0;

        Parent.Notify_OnProjectileFired();
    }

    #endregion

    #region Turret Helper Actions

    internal void Setup(TurretProperties props, int index, TurretGunSet set, ITurretHolder parent)
    {
        _props = props;
        _turretIndex = index;
        Parent = parent;

        //
        _burstCooldownTicksLeft = props.turretInitialCooldownTime.SecondsToTicks();

        _gun = ThingMaker.MakeThing(props.turretGunDef);
        List<Verb> allVerbs = Gun.TryGetComp<CompEquippable>().AllVerbs;
        foreach (var verb in allVerbs)
        {
            verb.caster = parent.Caster;
            verb.castCompleteCallback = BurstComplete;
            VerbWatcher.GetAttacher(verb).AttachTurret(this);
            // if(VerbWatcher.GetAttacher(verb).AttachTurret(this))
            // if (verb is Verb_Tele ve) 
            //     ve.turretGun = this;
        }

        //
        if (UsesTurretGunTop)
        {
            _top = new TurretGunTop(this, props.turretTop);
            int max1 = 1,
                max2 = 1;
            if (props.turretTop.barrels != null)
                max1 = props.turretTop.barrels.Count;
            /*
            if (AttackVerb.Props.originOffsets != null)
                max2 = AttackVerb.Props.originOffsets.Count;
            */
            maxShotRotations = Math.Max(max1, max2);
        }
    }

    private LocalTargetInfo TryFindNewTarget()
    {
        var attackTargetSearcher = TargSearcher();
        var faction = attackTargetSearcher.Thing.Faction;
        var range = AttackVerb.verbProps.range;
        Building t;
        if (Rand.Value < 0.5f && this.AttackVerb.ProjectileFliesOverhead() && faction.HostileTo(Faction.OfPlayer) && ParentThing.Map.listerBuildings
                .allBuildingsColonist.Where(delegate (Building x)
                {
                    var num = AttackVerb.verbProps.EffectiveMinRange(x, ParentThing);
                    float num2 = x.Position.DistanceToSquared(ParentThing.Position);
                    return num2 > num * num && num2 < range * range;
                }).TryRandomElement(out t))
            return t;
        var targetScanFlags = TargetScanFlags.NeedThreat | TargetScanFlags.NeedAutoTargetable;
        if (!AttackVerb.ProjectileFliesOverhead())
        {
            targetScanFlags |= TargetScanFlags.NeedLOSToAll;
            targetScanFlags |= TargetScanFlags.LOSBlockableByGas;
        }
        if (AttackVerb.IsIncendiary_Ranged())
        {
            targetScanFlags |= TargetScanFlags.NeedNonBurning;
        }

        if (NeedsRoofless)
        {
            targetScanFlags |= TargetScanFlags.NeedNotUnderThickRoof;
        }

        return (Thing)AttackTargetFinder.BestShootTargetFromCurrentPosition(attackTargetSearcher, targetScanFlags, IsValidTarget);
    }

    private bool IsValidTarget(Thing t)
    {
        if (t is Pawn pawn)
        {
            if (Parent.Faction == Faction.OfPlayer && pawn.IsPrisoner) return false;
            if (AttackVerb.ProjectileFliesOverhead())
            {
                var roofDef = ParentThing.Map.roofGrid.RoofAt(t.Position);
                if (roofDef is { isThickRoof: true }) return false;
            }

            if (Parent.MannableComp == null) return !GenAI.MachinesLike(Parent.Faction, pawn);
            if (pawn.RaceProps.Animal && pawn.Faction == Faction.OfPlayer) return false;
        }

        return true;
    }

    private void StartTargeting(LocalTargetInfo newTarget)
    {
        Parent.Notify_NewTarget(CurrentTarget);
    }

    private void ExtractShell()
    {
        var shell = Gun.TryGetComp<CompChangeableProjectile>().RemoveShell();
        GenPlace.TryPlaceThing(shell, ParentThing.Position, ParentThing.Map, ThingPlaceMode.Near);
    }

    internal void ResetForcedTarget()
    {
        Parent.Notify_LostTarget(_forcedTarget);
        _forcedTarget = LocalTargetInfo.Invalid;
        _burstWarmupTicksLeft = 0;
        if (_burstCooldownTicksLeft <= 0)
            TryStartShootSomething(false);
    }

    internal void ResetCurrentTarget()
    {
        Parent.Notify_LostTarget(_currentTargetInt);
        _currentTargetInt = LocalTargetInfo.Invalid;
        _burstWarmupTicksLeft = 0;
    }

    private void OnAttackedTarget(LocalTargetInfo target)
    {
        this._lastAttackTargetTick = Find.TickManager.TicksGame;
        this._lastAttackedTarget = target;
    }

    private IAttackTargetSearcher TargSearcher()
    {
        if (Parent.MannableComp is { MannedNow: true })
        {
            return Parent.MannableComp.ManningPawn;
        }
        return this;
    }

    private void BurstComplete()
    {
        _burstCooldownTicksLeft = BurstCooldownTime.SecondsToTicks();
    }

    public void TryOrderAttack(LocalTargetInfo targ)
    {
        if (!targ.IsValid)
        {
            if (_forcedTarget.IsValid)
                ResetForcedTarget();
            return;
        }

        if (!CanSetForcedTarget)
        {
            Messages.Message("Tele.TurretGunCantSetForced".Translate(), null, MessageTypeDefOf.RejectInput, false);
            return;
        }

        //Out of Range
        if ((targ.Cell - ParentThing.Position).LengthHorizontal < AttackVerb.verbProps.EffectiveMinRange(targ, Parent.Caster))
        {
            Messages.Message("MessageTargetBelowMinimumRange".Translate(), null, MessageTypeDefOf.RejectInput, false);
            return;
        }

        if ((targ.Cell - ParentThing.Position).LengthHorizontal > AttackVerb.verbProps.range)
        {
            Messages.Message("MessageTargetBeyondMaximumRange".Translate(), null, MessageTypeDefOf.RejectInput, false);
            return;
        }

        //
        if (_forcedTarget != targ)
        {
            _forcedTarget = targ;
            if (_burstCooldownTicksLeft <= 0)
                TryStartShootSomething(false);
        }

        //Holding Fire
        if (HoldFire)
        {
            Messages.Message("MessageTurretWontFireBecauseHoldFire".Translate(ParentThing.def.label), ParentThing,
                MessageTypeDefOf.RejectInput, false);
            ResetForcedTarget();
        }
    }

    public void ForceOrderAttack(LocalTargetInfo targ)
    {
        if (!targ.IsValid)
        {
            if (_forcedTarget.IsValid)
                ResetForcedTarget();
            return;
        }

        if ((targ.Cell - ParentThing.Position).LengthHorizontal < AttackVerb.verbProps.EffectiveMinRange(targ, Parent.Caster))
        {
            Messages.Message("MessageTargetBelowMinimumRange".Translate(), null, MessageTypeDefOf.RejectInput, false);
            return;
        }

        if ((targ.Cell - ParentThing.Position).LengthHorizontal > AttackVerb.verbProps.range)
        {
            Messages.Message("MessageTargetBeyondMaximumRange".Translate(), null, MessageTypeDefOf.RejectInput, false);
            return;
        }

        _currentTargetInt = targ;
        AttackVerb.TryStartCastOn(targ);
        OnAttackedTarget(targ);

        if (HoldFire)
        {
            Messages.Message("MessageTurretWontFireBecauseHoldFire".Translate(ParentThing.def.label), ParentThing,
                MessageTypeDefOf.RejectInput, false);
            ResetForcedTarget();
        }
    }

    #endregion
}

/*{
public class TurretGun3 : IAttackTarget, IAttackTargetSearcher
{
    protected int burstCooldownTicksLeft;

    protected int burstWarmupTicksLeft;
    protected LocalTargetInfo currentTargetInt = LocalTargetInfo.Invalid;

    //

    //Dynamic Data

    private int lastShotIndex;
    protected LocalTargetInfo localForcedTarget = LocalTargetInfo.Invalid;
    private int maxShotRotations = 1;
    protected Effecter progressBarEffecter;
    protected TurretProperties props;

    //SubComps
    protected TurretGunTop top;

    //Settings
    private int turretIndex;

    //
    public LocalTargetInfo CurrentTarget => currentTargetInt;

    public Thing Caster => ParentHolder.Caster;
    public Thing ParentThing => ParentHolder.HolderThing;

    //
    public TurretProperties Props => props;
    public TurretGunSet ParentSet { get; private set; }
    public ITurretHolder ParentHolder { get; private set; }

    //
    public int BurstCoolDownTicksLeft => burstCooldownTicksLeft;
    public int BurstWarmupTicksLeft => burstWarmupTicksLeft;

    public CompPowerTrader PowerComp => ParentHolder.PowerTraderComp;
    public CompCanBeDormant DormantComp => ParentHolder.DormantComp;
    public CompInitiatable InitiatableComp => ParentHolder.InitiatableComp;
    public CompRefuelable RefuelComp => ParentHolder.RefuelComp;
    public CompNetwork NetworkComp => ParentHolder.NetworkComp;
    public CompMannable MannableComp => ParentHolder.MannableComp;

    //Basic Turret
    public CompEquippable GunCompEq => Gun.TryGetComp<CompEquippable>();
    private bool WarmingUp => burstWarmupTicksLeft > 0;
    public Verb AttackVerb => GunCompEq.PrimaryVerb;
    public VerbProperties VerbProps => AttackVerb.verbProps;
    public Verb_Tele TeleVerb => AttackVerb as Verb_Tele;
    public VerbProperties_Extended VerbPropsExtended => TeleVerb.Props;
    public bool IsMannable => MannableComp != null;
    public bool PlayerControlled => ParentHolder.PlayerControlled;
    public bool CanSetForcedTarget => (MannableComp != null || props.canForceTarget) && PlayerControlled;
    public bool CanToggleHoldFire => PlayerControlled;

    private bool IsMortar => ParentThing.def.building.IsMortar || AttackVerb is Verb_Tele {IsMortar: true};
    private bool IsMortarOrProjectileFliesOverhead => AttackVerb.ProjectileFliesOverhead() || IsMortar;

    private bool CanExtractShell
    {
        get
        {
            if (!PlayerControlled)
                return false;
            var compChangeableProjectile = Gun.TryGetComp<CompChangeableProjectile>();
            return compChangeableProjectile is {Loaded: true};
        }
    }

    private bool HoldFire => ParentSet.HoldingFire;

    //
    private Pawn ManningPawn => MannableComp?.ManningPawn;
    private bool MannedByColonist => ManningPawn?.Faction == Faction.OfPlayer;

    public Thing Gun { get; private set; }
    public float TurretRotation => top?.CurRotation ?? 0;
    public int ShotIndex { get; private set; }

    public bool Continuous => props.continuous;
    public bool NeedsRoof => IsMortar;

    public TurretGunTop Top => top;
    public bool UsesTurretGunTop => props.turretTop != null;

    public Graphic TurretGraphic => props.turretTop.topGraphic.Graphic;
    public Vector3 DrawPos => ParentThing.DrawPos + props.turretOffset;

    public LocalTargetInfo TargetCurrentlyAimingAt => currentTargetInt;

    //IAttackTarget.Thing | IAttackTargetSearcher.Thing
    public Thing Thing => ParentThing;

    public float TargetPriorityFactor => 1f;

    public bool ThreatDisabled(IAttackTargetSearcher disabledFor)
    {
        return ParentHolder.ThreatDisabled(disabledFor);
    }

    public string GetUniqueLoadID()
    {
        return $"{ParentThing.ThingID}_TurretGun";
    }

    public LocalTargetInfo LastAttackedTarget { get; private set; }

    //public Verb_Extended AttackVerb => (Verb_Extended)CurrentEffectiveVerb;

    public Verb CurrentEffectiveVerb => GunCompEq.PrimaryVerb;
    public int LastAttackTargetTick { get; private set; }

    internal void Setup(TurretProperties props, int index, TurretGunSet set, ITurretHolder parent)
    {
        turretIndex = index;
        this.props = props;
        ParentSet = set;
        ParentHolder = parent;
        //
        burstCooldownTicksLeft = props.turretInitialCooldownTime.SecondsToTicks();
        MakeGun();

        //
        if (UsesTurretGunTop)
        {
            top = new TurretGunTop(this, props.turretTop);
            int max1 = 1,
                max2 = 1;
            if (props.turretTop.barrels != null)
                max1 = props.turretTop.barrels.Count;
            /*
            if (AttackVerb.Props.originOffsets != null)
                max2 = AttackVerb.Props.originOffsets.Count;
            #1#
            maxShotRotations = Math.Max(max1, max2);
        }
    }

    public void Cleanup()
    {
        currentTargetInt = LocalTargetInfo.Invalid;
        burstWarmupTicksLeft = 0;
        progressBarEffecter.Cleanup();
    }

    //
    private void StartTargeting(LocalTargetInfo newTarget)
    {
        ParentSet.Notify_NewTarget(CurrentTarget);
    }

    public void OrderAttackNow(LocalTargetInfo targ)
    {
        if (!targ.IsValid)
        {
            if (localForcedTarget.IsValid) 
                ResetForcedTarget();
            return;
        }
        
        if ((targ.Cell - ParentThing.Position).LengthHorizontal < AttackVerb.verbProps.EffectiveMinRange(targ, Caster))
        {
            Messages.Message("MessageTargetBelowMinimumRange".Translate(), null, MessageTypeDefOf.RejectInput, false);
            return;
        }

        if ((targ.Cell - ParentThing.Position).LengthHorizontal > AttackVerb.verbProps.range)
        {
            Messages.Message("MessageTargetBeyondMaximumRange".Translate(), null, MessageTypeDefOf.RejectInput, false);
            return;
        }

        currentTargetInt = targ;
        AttackVerb.TryStartCastOn(targ);
        OnAttackedTarget(targ);
        
        if (HoldFire)
        {
            Messages.Message("MessageTurretWontFireBecauseHoldFire".Translate(ParentThing.def.label), ParentThing,
                MessageTypeDefOf.RejectInput, false);
            ResetForcedTarget();
        }
    }
    
    public void TryOrderAttack(LocalTargetInfo targ)
    {
        if (!targ.IsValid)
        {
            if (localForcedTarget.IsValid) ResetForcedTarget();
            return;
        }

        if (!CanSetForcedTarget)
        {
            Messages.Message("Tele.TurretGunCantSetForced".Translate(), null, MessageTypeDefOf.RejectInput, false);
            return;
        }

        //Out of Range
        if ((targ.Cell - ParentThing.Position).LengthHorizontal < AttackVerb.verbProps.EffectiveMinRange(targ, Caster))
        {
            Messages.Message("MessageTargetBelowMinimumRange".Translate(), null, MessageTypeDefOf.RejectInput, false);
            return;
        }

        if ((targ.Cell - ParentThing.Position).LengthHorizontal > AttackVerb.verbProps.range)
        {
            Messages.Message("MessageTargetBeyondMaximumRange".Translate(), null, MessageTypeDefOf.RejectInput, false);
            return;
        }

        //
        if (localForcedTarget != targ)
        {
            localForcedTarget = targ;
            if (burstCooldownTicksLeft <= 0)
                TryStartShootSomething(false);
        }

        //Holding Fire
        if (HoldFire)
        {
            Messages.Message("MessageTurretWontFireBecauseHoldFire".Translate(ParentThing.def.label), ParentThing,
                MessageTypeDefOf.RejectInput, false);
            ResetForcedTarget();
        }
    }

    public void TurretTick()
    {
        if (CanExtractShell && MannedByColonist)
        {
            var compChangeableProjectile = Gun.TryGetComp<CompChangeableProjectile>();
            if (!compChangeableProjectile.allowedShellsSettings.AllowedToAccept(compChangeableProjectile.LoadedShell))
                ExtractShell();
        }

        //Reset Forced
        if (localForcedTarget.ThingDestroyed || (localForcedTarget.IsValid && !CanSetForcedTarget)) ResetForcedTarget();

        if (!ParentHolder.Active)
        {
            ResetCurrentTarget();
            return;
        }

        //Turret Active
        top?.TurretTopTick();
        GunCompEq.verbTracker.VerbsTick();
        if (!ParentHolder.IsStunned && AttackVerb.state != VerbState.Bursting)
        {
            if (Continuous)
            {
                TryStartShootSomething(true);
            }
            else if (WarmingUp)
            {
                if (burstWarmupTicksLeft == props.turretBurstWarmupTime.SecondsToTicks() - 1)
                    //Play Warmup Charge, if available
                    VerbPropsExtended?.chargeSound?.PlayOneShot(SoundInfo.InMap(new TargetInfo(ParentThing)));

                burstWarmupTicksLeft--;
                if (burstWarmupTicksLeft == 0) BeginBurst();
            }
            else
            {
                if (burstCooldownTicksLeft > 0)
                {
                    burstCooldownTicksLeft--;
                    if (IsMortar)
                    {
                        if (progressBarEffecter == null) progressBarEffecter = EffecterDefOf.ProgressBar.Spawn();
                        progressBarEffecter.EffectTick(ParentThing, TargetInfo.Invalid);
                        var mote = ((SubEffecter_ProgressBar) progressBarEffecter.children[0]).mote;
                        mote.progress = 1f - Math.Max(burstCooldownTicksLeft, 0) /
                            (float) BurstCooldownTime().SecondsToTicks();
                        mote.offsetZ = -0.8f;
                    }
                }

                if (burstCooldownTicksLeft <= 0 &&
                    ParentThing.IsHashIntervalTick(VerbPropsExtended?.shotIntervalTicks ?? 10))
                    TryStartShootSomething(false);
            }
        }
    }

    protected void TryStartShootSomething(bool canBeginBurstImmediately)
    {
        if (progressBarEffecter != null)
        {
            progressBarEffecter.Cleanup();
            progressBarEffecter = null;
        }

        if (!ParentThing.Spawned || (HoldFire && CanToggleHoldFire) ||
            (NeedsRoof && ParentThing.Map.roofGrid.Roofed(ParentThing.Position)) || !AttackVerb.Available())
        {
            ResetCurrentTarget();
            return;
        }

        currentTargetInt = localForcedTarget.IsValid ? localForcedTarget : TryFindNewTarget();

        if (CurrentTarget.IsValid)
        {
            StartTargeting(currentTargetInt);

            if (!top?.OnTarget ?? false) return;
            if (canBeginBurstImmediately)
                BeginBurst();
            else if (props.turretBurstWarmupTime > 0f)
                burstWarmupTicksLeft = props.turretBurstWarmupTime.SecondsToTicks();
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
        if (Rand.Value < 0.5f && NeedsRoof && faction.HostileTo(Faction.OfPlayer) && ParentThing.Map.listerBuildings
                .allBuildingsColonist.Where(delegate(Building x)
                {
                    var num = AttackVerb.verbProps.EffectiveMinRange(x, ParentThing);
                    float num2 = x.Position.DistanceToSquared(ParentThing.Position);
                    return num2 > num * num && num2 < range * range;
                }).TryRandomElement(out t))
            return t;
        var targetScanFlags = TargetScanFlags.NeedThreat;
        if (!NeedsRoof)
        {
            targetScanFlags |= TargetScanFlags.NeedLOSToAll;
            targetScanFlags |= TargetScanFlags.LOSBlockableByGas;
        }

        if (AttackVerb.IsIncendiary()) targetScanFlags |= TargetScanFlags.NeedNonBurning;
        return (Thing) AttackTargetFinder.BestShootTargetFromCurrentPosition(attackTargetSearcher, targetScanFlags,
            IsValidTarget);
    }

    private IAttackTargetSearcher TargSearcher()
    {
        if (MannableComp is {MannedNow: true})
            return MannableComp.ManningPawn;
        return this;
    }

    private bool IsValidTarget(Thing t)
    {
        if (t is not Pawn pawn) return true;
        /*
        if(tiberium.burstMode == TurretBurstMode.ToTarget && tiberium.avoidFriendlyFire)
        {
            ShootLine line = new ShootLine(parent.Position, pawn.Position);
            if(line.Points().Any(P => P.GetFirstBuilding(parent.Map) is Building b && b != parent && b.Faction.IsPlayer))
            {
                return false;
            }
        }
        #1#
        if (NeedsRoof)
        {
            var roofDef = ParentThing.Map.roofGrid.RoofAt(t.Position);
            if (roofDef is {isThickRoof: true}) return false;
        }

        if (MannableComp == null) return !GenAI.MachinesLike(ParentThing.Faction, pawn);
        /*
        if (ParentHolder.CurrentTarget != null && ParentHolder.CurrentTarget.Thing != t)
            return false;
        if(ParentHolder.HasTarget(t))
        #1#
        if (pawn.RaceProps.Animal && pawn.Faction == Faction.OfPlayer) return false;

        //Ignore already chosen targets | this will always choose only one target, needs smarter search
        //if (ParentSet.HasTarget(t)) return false;

        return true;
    }

    protected void BeginBurst()
    {
        AttackVerb.TryStartCastOn(CurrentTarget);
        OnAttackedTarget(CurrentTarget);
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

    //
    private void MakeGun()
    {
        Gun = ThingMaker.MakeThing(props.turretGunDef);
        List<Verb> allVerbs = Gun.TryGetComp<CompEquippable>().AllVerbs;
        foreach (var verb in allVerbs)
        {
            verb.caster = ParentHolder.Caster;
            verb.castCompleteCallback = BurstComplete;
            if (verb is Verb_Tele ve) ve.turretGun = this;
        }
        UpdateGunVerbs();
    }

    private void UpdateGunVerbs()
    {
        List<Verb> allVerbs = Gun.TryGetComp<CompEquippable>().AllVerbs;
        foreach (var verb in allVerbs)
        {
            verb.caster = ParentHolder.Caster;
            verb.castCompleteCallback = BurstComplete;
            if (verb is Verb_Tele ve) ve.turretGun = this;
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

    private void ExtractShell()
    {
        GenPlace.TryPlaceThing(Gun.TryGetComp<CompChangeableProjectile>().RemoveShell(), ParentThing.Position,
            ParentThing.Map, ThingPlaceMode.Near);
    }

    public void ResetForcedTarget()
    {
        ParentSet.Notify_LostTarget(localForcedTarget);
        localForcedTarget = LocalTargetInfo.Invalid;
        burstWarmupTicksLeft = 0;
        if (burstCooldownTicksLeft <= 0)
            TryStartShootSomething(false);
    }

    public void ResetCurrentTarget()
    {
        ParentSet.Notify_LostTarget(currentTargetInt);
        currentTargetInt = LocalTargetInfo.Invalid;
        burstWarmupTicksLeft = 0;
    }

    public void Notify_FiredSingleProjectile()
    {
        top?.Notify_TurretShot(ShotIndex);
        RotateNextShotIndex();
        ParentHolder.Notify_OnProjectileFired();
    }

    private void RotateNextShotIndex()
    {
        lastShotIndex = ShotIndex;
        ShotIndex++;
        if (ShotIndex > maxShotRotations - 1)
            ShotIndex = 0;
    }

    //
    public void Draw()
    {
        if (UsesTurretGunTop)
            top.DrawTurret();
        if (Find.Selector.IsSelected(ParentThing))
            DrawSelectionOverlays();
    }

    private void DrawSelectionOverlays()
    {
        if (localForcedTarget.IsValid && (!localForcedTarget.HasThing || localForcedTarget.Thing.Spawned))
        {
            var b = localForcedTarget.HasThing ? localForcedTarget.Thing.TrueCenter() : localForcedTarget.CenterVector3;
            var a = DrawPos;
            b.y = AltitudeLayer.MetaOverlays.AltitudeFor();
            a.y = b.y;
            GenDraw.DrawLineBetween(a, b, TeleContent.ForcedTargetLineMat);
        }

        var range = AttackVerb.verbProps.range;
        if (range < 90f) GenDraw.DrawRadiusRing(ParentThing.Position, range);
        var num = AttackVerb.verbProps.EffectiveMinRange(true);
        if (num < 90f && num > 0.1f) GenDraw.DrawRadiusRing(ParentThing.Position, num);

        if (UsesTurretGunTop && WarmingUp)
        {
            var degreesWide = (int) (burstWarmupTicksLeft * 0.5f);
            GenDraw.DrawAimPieRaw(DrawPos + (TeleVerb?.BaseDrawOffsetRotated ?? Vector3.zero), TurretRotation,
                degreesWide);
        }
    }
}
}*/