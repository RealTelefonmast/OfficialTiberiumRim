using RimWorld;
using UnityEngine;
using Verse;

namespace TeleCore.Systems.Verbs;

/// <summary>
/// A data container attached to a Verb to handle custom logic.
/// </summary>
public class TeleVerbAttacher
{
    private IVerbOwner _owner;
    private Verb _verb;

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

    public VerbProperties_Tele Props => (VerbProperties_Tele)_verb.verbProps!;

    public ThingDef Projectile
    {
        get
        {
            if (_changeableProjectile is { Loaded: true })
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

    public TeleVerbAttacher(IVerbOwner owner, Verb verb)
    {
        _owner = owner;
        _verb = verb;

        GenerateOffset();
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
    
    private void GetCompData()
    {
        _changeableProjectile = _verb.EquipmentSource?.GetComp<CompChangeableProjectile>();
    }

    internal void AttachTurret(TurretGun turretGun)
    {
        _turretGun = turretGun;
    }

    #region Logic Hooks

    public void Notify_WarmupComplete()
    {
    }

    public void Notify_ShotCast()
    {
    }

    public void Notify_Reset()
    {
    }

    public void Notify_SingleShot(VerbArgs args)
    {
        Props.originEffecter?.Spawn(Caster.Position, Caster.Map, DrawPosOffset);
        
        if (Props.muzzleFlash != null)
        {
            TeleVerbUtilities.DoMuzzleFlash(Caster.Map, RelativeDrawOffset, args.IntendedTarget, Props.muzzleFlash);
        }

        if (args.Projectile != null)
        {
            _turretGun?.Notify_FiredSingleProjectile();
        }
        
        RotateNextShotIndex();
    }

    public struct VerbArgs
    {
        public Projectile? Projectile { get; set; }
        public LocalTargetInfo IntendedTarget { get; set; }
    }

    public void Notify_ProjectileLaunched(Projectile projectile)
    {
    }

    #endregion
    
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