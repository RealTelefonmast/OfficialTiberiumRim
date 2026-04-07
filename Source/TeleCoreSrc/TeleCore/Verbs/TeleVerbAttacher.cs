using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace TeleCore.Verbs;

/// <summary>
/// A data container attached to vanilla verbs as an outside data component!
/// </summary>
public class TeleVerbAttacher
{
    private IVerbOwner _owner;
    private Verb _verb;
    private List<VerbComponent> _comps;
    
    private CompChangeableProjectile _changeableProjectile;
    
    //
    private ThingDef _projectileOverride;
    private int _curShotIndex;
    private int _lastShotIndex;
    private int _maxShotRotations = 1;
    
    private TurretGun? _turretGun;
    private Vector3[]? _drawOffsets;
    
    public Verb Verb => _verb;

    public bool CasterIsPawn => _verb.CasterIsPawn;
    public Pawn CasterPawn => _verb.CasterPawn;
    public Thing Caster => _verb.Caster;
    
    public bool Available => _verb.Available();

    public VerbProperties_Tele Props => (VerbProperties_Tele)_verb.verbProps;
    
    public ThingDef Projectile
    {
        get
        {
            if (_changeableProjectile is {Loaded: true}) 
                return _changeableProjectile.Projectile;
            return _projectileOverride;
        }
    }

    public DamageDef DamageDef
    {
        get
        {
            //TODO: Add other variations and cases like beam
            return Projectile.projectile.damageDef;
        }
    }
    
    public event Action WarmupComplete;
    public event Action ShotCast;
    public event Action Reset;
    public event Action<Projectile> ProjectileLaunched;

    public TeleVerbAttacher(IVerbOwner owner, Verb verb)
    {
        _owner = owner;
        _verb = verb;

        GenerateOffset();
        GenerateVerbComps();
        GetCompData();
    }

    private void GenerateOffset()
    {
        if (Props.originOffsetPerShot != null)
        {
            _drawOffsets = new Vector3[Props.originOffsetPerShot.Count];
            for (var i = 0; i < Props.originOffsetPerShot.Count; i++)
            {
                var offset = Props.originOffsetPerShot[i];
                _drawOffsets[i] = Props.shotStartOffset + offset;
            }
        }
    }

    private void GenerateVerbComps()
    {
        if (Props.comps.NullOrEmpty()) return;
        _comps = new List<VerbComponent>();
        foreach (var compProp in Props.comps)
        {
            var comp = (VerbComponent) Activator.CreateInstance(compProp.compClass);
            WarmupComplete += comp.Notify_WarmupComplete;
            ShotCast += comp.Notify_ShotCast;
            Reset += comp.Notify_Reset;
            ProjectileLaunched += comp.Notify_ProjectileLaunched;
            _comps.Add(comp);
        }
    }

    private void GetCompData()
    {
        _changeableProjectile = _verb.EquipmentSource?.GetComp<CompChangeableProjectile>();
    }

    internal void AttachTurret(TurretGun turretGun)
    {
        _turretGun = turretGun;
    }
    
    public void Notify_WarmupComplete()
    {
        WarmupComplete?.Invoke();
    }
    
    public void Notify_ShotCast()
    {
        ShotCast?.Invoke();
    }
    
    public void Notify_Reset()
    {
        Reset?.Invoke();
    }

    public void Notify_ProjectileLaunched(Projectile projectile)
    {
        ProjectileLaunched?.Invoke(projectile);
        //Do Origin Effect
        Props.originEffecter?.Spawn(Caster.Position, Caster.Map, DrawPosOffset);
        //Do Muzzle Flash
        TeleVerbUtilities.DoMuzzleFlash(Caster.Map, RelativeDrawOffset, projectile.intendedTarget, Props.muzzleFlash);
        
        _turretGun?.Notify_FiredSingleProjectile();
        RotateNextShotIndex();
    }

    #region Data

    public float DesiredAimAngle
    {
        get
        {
            if (_turretGun != null) return _turretGun.TurretRotation;

            if (!CasterIsPawn) return 0;
            if (CasterPawn.stances.curStance is not Stance_Busy stance_Busy) return 0;

            //
            Vector3 targetPos;
            if (stance_Busy.focusTarg.HasThing)
                targetPos = stance_Busy.focusTarg.Thing.DrawPos;
            else
                targetPos = stance_Busy.focusTarg.Cell.ToVector3Shifted();

            if ((targetPos - CasterPawn.DrawPos).MagnitudeHorizontalSquared() > 0.001f)
                return (targetPos - CasterPawn.DrawPos).AngleFlat();

            return 0;
        }
    }
    
    protected float CurrentAimAngle
    {
        get
        {
            if (CasterIsPawn) return DesiredAimAngle;
            return _turretGun?.TurretRotation ?? 0f;
        }
    }

    #endregion
    
    #region Origin Rotation
    
    protected int ShotIndex => _turretGun?.ShotIndex ?? _curShotIndex;
    
    private Vector3 BaseOrigin => Verb.caster.DrawPos; //turretGun?.DrawPos ??

    private Vector3 DrawPosOffset
    {
        get
        {
            if (_turretGun != null)
                return _turretGun.Props.turretOffset + Props.shotStartOffset.RotatedBy(CurrentAimAngle);
            return Vector3.zero;
        }
    }
    
    public Vector3 BaseDrawOffset => DrawPosOffset + Props.shotStartOffset.RotatedBy(CurrentAimAngle);
    
    public Vector3 RelativeDrawOffset
    {
        get
        {
            if (Props.originOffsetPerShot == null) return BaseDrawOffset;
            var curOffset = Props.originOffsetPerShot[ShotIndex];
            return (BaseDrawOffset + curOffset).RotatedBy(CurrentAimAngle);
        }
    }

    public Vector3 CurrentOrigin => BaseOrigin + RelativeDrawOffset;

    #endregion

    private void RotateNextShotIndex()
    {
        _lastShotIndex = _curShotIndex;
        _curShotIndex = _curShotIndex >= _maxShotRotations - 1 ? 0 : _curShotIndex + 1;
    }
    
    /// <summary>
    /// When certain configs are set, we allow manipulating the launch origin of projectiles.
    /// </summary>
    public Vector3 GetLaunchPosition(Vector3 initial)
    {
        initial += Props.shotStartOffset;
        if (Props.originOffsetPerShot != null)
        {
            initial += Props.originOffsetPerShot[ShotIndex].RotatedBy(CurrentAimAngle);
        }
        return initial;
    }
}