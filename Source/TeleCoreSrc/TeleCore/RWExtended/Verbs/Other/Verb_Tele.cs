using System.Collections.Generic;
using RimWorld;
using TeleCore.Static;
using TeleCore.ThingComps;
using UnityEngine;
using Verse;
using Verse.AI;

namespace TeleCore.RWExtended.Verbs.Other;

public abstract class Verb_Tele : Verb
{
    private readonly List<Vector3>? _drawOffsets = [];

    private int currentOffsetIndex;

    //Turret Barrel Offset
    private int lastOffsetIndex;
    private int maxOffsetCount = 1;

    //
    public TurretGun turretGun;

    //
    public VerbProperties_Extended Props => (VerbProperties_Extended)verbProps;
    public CompNetwork NetworkComp => caster.TryGetComp<CompNetwork>();
    public CompPowerTrader PowerComp => caster.TryGetComp<CompPowerTrader>();

    public ThingDef GunDef
    {
        get
        {
            if (CasterIsPawn)
                return EquipmentSource.def;
            if (turretGun != null)
                return turretGun.Gun.def;
            return caster.def.building.turretGunDef;
        }
    }

    public bool IsBeam => Props.beamProps != null;
    public bool IsMortar => !IsBeam && Props.defaultProjectile.projectile.flyOverhead;

    public virtual DamageDef DamageDef => null;
    public virtual ThingDef Projectile => null;
    protected virtual float ExplosionOnTargetSize => 0;

    public override int ShotsPerBurst => verbProps.burstShotCount;

    //Origin Offsetting
    protected int OffsetIndex => turretGun?.ShotIndex ?? currentOffsetIndex;

    //Angles and Deviles
    public float DesiredAimAngle
    {
        get
        {
            if (turretGun != null) return turretGun.TurretRotation;

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
            /*
            if (AimAngleOverride != null)
                return AimAngleOverride.Value;
            */
            if (CasterIsPawn) return DesiredAimAngle;
            return turretGun?.TurretRotation ?? 0f;
        }
    }

    //REAL DrawPos
    protected Vector3 DrawPos => caster.DrawPos; //turretGun?.DrawPos ??

    public Vector3 DrawPosOffset
    {
        get
        {
            if (turretGun != null)
                return turretGun.Props.turretOffset + Props.shotStartOffset.RotatedBy(CurrentAimAngle);
            return Vector3.zero;
        }
    }

    public Vector3 BaseDrawOffset => DrawPosOffset + Props.shotStartOffset.RotatedBy(CurrentAimAngle);

    public Vector3 BaseDrawOffsetRotated => BaseDrawOffset.RotatedBy(CurrentAimAngle);

    public List<Vector3>? RelativeDrawOffsets
    {
        get
        {
            _drawOffsets!.Clear();
            var baseOff = BaseDrawOffset;
            if (Props.originOffsetPerShot != null)
                foreach (var vector3 in Props.originOffsetPerShot)
                    _drawOffsets.Add(baseOff + vector3.RotatedBy(CurrentAimAngle));
            else
                return null;
            return _drawOffsets;
        }
    }

    public Vector3 CurrentDrawOffset => RelativeDrawOffsets?[OffsetIndex] ?? BaseDrawOffset;
    public Vector3 CurrentStartPos => DrawPos + CurrentDrawOffset;

    protected virtual Vector3 ShotOriginLOS => CurrentStartPos + new Vector3(0, Props.shootHeightOffset, 0);

    public virtual void DrawVerb(Vector3 drawPos)
    {
        var altOff = new Vector3(0, AltitudeLayer.Projectile.AltitudeFor(), 0);

        var baseOff = BaseDrawOffset + altOff;
        var baseMatrix = new Matrix4x4();
        baseMatrix.SetTRS(baseOff, CurrentAimAngle.ToQuat(), new Vector3(0.5f, 0, 0.5f));
        Graphics.DrawMesh(MeshPool.plane10, baseMatrix, TeleContent.IOArrow, 0);


        /* DEBUG Shot Origin Drawer
        foreach (var drawOffset in RelativeDrawOffsets)
        {
            var off = DrawPos + drawOffset + altOff;
            Matrix4x4 matri = new Matrix4x4();
            matri.SetTRS(off, CurrentAimAngle.ToQuat(), new Vector3(0.5f, 0, 0.5f));
            Graphics.DrawMesh(MeshPool.plane10, matri, BaseContent.BadMat, 0);
        }
        */
    }

    //Barrel Rotation
    private void RotateNextShotIndex()
    {
        lastOffsetIndex = currentOffsetIndex;
        currentOffsetIndex++;
        if (currentOffsetIndex >= maxOffsetCount)
            currentOffsetIndex = 0;
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref lastOffsetIndex, "lastOffsetIndex");
        Scribe_Values.Look(ref currentOffsetIndex, "currentOffsetIndex");
        Scribe_Values.Look(ref maxOffsetCount, "maxOffsetCount");
    }

    public override void Reset()
    {
        base.Reset();
        maxOffsetCount = Props.originOffsetPerShot?.Count ?? 1;
        currentOffsetIndex = 0;
        lastOffsetIndex = 0;
    }

    private void Notify_SingleShot()
    {
        turretGun?.Notify_FiredSingleProjectile();
        RotateNextShotIndex();
    }

    //
    public override bool IsUsableOn(Thing target)
    {
        return true;
    }

    protected virtual bool TryCastAttack()
    {
        return false;
    }

    protected virtual bool IsAvailable()
    {
        return true;
    }

    protected virtual BattleLogEntry_RangedFire EntryOnWarmupComplete()
    {
        return null;
    }

    public virtual void PreVerbTick()
    {
    }

    public virtual void PostVerbTick()
    {
    }

    //Base Tele Behaviour
    public override void WarmupComplete()
    {
        burstShotsLeft = ShotsPerBurst;
        state = VerbState.Bursting;
        TryCastNextBurstShot();
        var entry = EntryOnWarmupComplete();
        if (entry != null) Find.BattleLog.Add(entry);
    }

    public sealed override bool Available()
    {
        if (!base.Available()) return false;

        //TODO: Add power consumption
        /*
        if (Props.powerConsumptionPerShot > 0)
        {
            PowerComp.PowerNet.batteryComps.Any(t => t.StoredEnergy > Props.powerConsumptionPerShot);
        }
        */

        if (Props.networkCostPerShot != null) return Props.networkCostPerShot.CanPayWith(NetworkComp);

        if (CasterIsPawn)
        {
            var casterPawn = CasterPawn;
            if (casterPawn.Faction != Faction.OfPlayer && casterPawn.mindState.MeleeThreatStillThreat &&
                casterPawn.mindState.meleeThreat.Position.AdjacentTo8WayOrInside(casterPawn.Position))
                return false;
        }

        return IsAvailable();
    }

    public sealed override bool TryCastShot()
    {
        var flag = TryCastAttack();
        if (!flag) return false;

        //Do Origin Effect if exists
        Props.originEffecter?.Spawn(caster.Position, caster.Map, DrawPosOffset);

        //Did Shot
        Notify_SingleShot();

        if (EquipmentSource != null)
        {
            var comp = EquipmentSource.GetComp<CompChangeableProjectile>();
            if (comp != null) comp.Notify_ProjectileLaunched();
            // 1.5 fucked this
            // var comp2 = EquipmentSource.GetComp<CompReloadable>();
            // var comp = EquipmentSource.GetComp<CompEquippableAbilityReloadable>()
            // if (comp2 != null) comp2.UsedOnce();
        }

        if (verbProps.consumeFuelPerShot > 0f)
            turretGun?.Parent.RefuelComp?.ConsumeFuel(verbProps.consumeFuelPerShot);

        //TODO: Add power consumption
        if (Props.powerConsumptionPerShot > 0)
        {
        }

        if (Props.networkCostPerShot != null)
        {
            if (Props.networkCostPerShot.CanPayWith(NetworkComp))
                Props.networkCostPerShot.DoPayWith(NetworkComp);
            else
                return false;
        }

        if (base.CasterIsPawn) base.CasterPawn.records.Increment(RecordDefOf.ShotsFired);
        return true;
    }

    public bool TryFindShootLineFromToNew(IntVec3 root, LocalTargetInfo targ, out ShootLine resultingLine)
    {
        if (targ.HasThing && targ.Thing.Map != caster.Map)
        {
            resultingLine = default;
            return false;
        }

        if (verbProps.IsMeleeAttack || EffectiveRange <= 1.42f)
        {
            resultingLine = new ShootLine(root, targ.Cell);
            return ReachabilityImmediate.CanReachImmediate(root, targ, caster.Map, PathEndMode.Touch, null);
        }

        var occupiedRect = targ.HasThing ? targ.Thing.OccupiedRect() : CellRect.SingleCell(targ.Cell);
        if (OutOfRange(root, targ, occupiedRect))
        {
            resultingLine = new ShootLine(root, targ.Cell);
            return false;
        }

        if (!verbProps.requireLineOfSight)
        {
            resultingLine = new ShootLine(root, targ.Cell);
            return true;
        }

        if (CasterIsPawn)
        {
            if (CanHitFromCellIgnoringRange(root, targ, out var dest))
            {
                resultingLine = new ShootLine(root, dest);
                return true;
            }

            ShootLeanUtility.LeanShootingSourcesFromTo(root, occupiedRect.ClosestCellTo(root), caster.Map,
                tempLeanShootSources);
            for (var i = 0; i < tempLeanShootSources.Count; i++)
            {
                var intVec = tempLeanShootSources[i];
                if (CanHitFromCellIgnoringRange(intVec, targ, out dest))
                {
                    resultingLine = new ShootLine(intVec, dest);
                    return true;
                }
            }
        }
        else
        {
            foreach (var intVec2 in caster.OccupiedRect())
            {
                IntVec3 dest;
                if (CanHitFromCellIgnoringRange(intVec2, targ, out dest))
                {
                    resultingLine = new ShootLine(intVec2, dest);
                    return true;
                }
            }
        }

        resultingLine = new ShootLine(root, targ.Cell);
        return false;
    }

    /// <summary>
    ///     Applies the vanilla target "miss" chance on an intended target
    /// </summary>
    protected LocalTargetInfo AdjustedTarget(LocalTargetInfo intended, ref ShootLine shootLine,
        out ProjectileHitFlags flags)
    {
        flags = ProjectileHitFlags.NonTargetWorld;
        if (verbProps.ForcedMissRadius > 0.5f)
        {
            var num = VerbUtility.CalculateAdjustedForcedMiss(verbProps.ForcedMissRadius,
                intended.Cell - caster.Position);
            if (num > 0.5f)
            {
                if (Rand.Chance(0.5f))
                    flags = ProjectileHitFlags.All;
                if (!canHitNonTargetPawnsNow)
                    flags &= ~ProjectileHitFlags.NonTargetPawns;

                var max = GenRadial.NumCellsInRadius(num);
                var num2 = Rand.Range(0, max);
                if (num2 > 0) return GetTargetFromPos(intended.Cell + GenRadial.RadialPattern[num2], caster.Map);
            }
        }

        var shotReport = ShotReport.HitReportFor(caster, this, intended);
        var cover = shotReport.GetRandomCoverToMissInto();
        if (!Rand.Chance(shotReport.AimOnTargetChance_IgnoringPosture))
        {
            if (Rand.Chance(0.5f) && canHitNonTargetPawnsNow)
                flags |= ProjectileHitFlags.NonTargetPawns;
            shootLine.ChangeDestToMissWild(shotReport.AimOnTargetChance_StandardTarget, caster.Map);
            return GetTargetFromPos(shootLine.Dest, caster.Map);
        }

        if (intended.Thing != null && intended.Thing.def.category == ThingCategory.Pawn &&
            !Rand.Chance(shotReport.PassCoverChance))
        {
            if (canHitNonTargetPawnsNow)
                flags |= ProjectileHitFlags.NonTargetPawns;
            return cover;
        }

        return intended;
    }

    protected Vector3 AdjustTargetDirect(Vector3 intended)
    {
        var intendedIntVec = intended.ToIntVec3();
        var diffLost = intended - intendedIntVec.ToVector3Shifted();

        if (verbProps.ForcedMissRadius > 0.5f)
        {
            var num = VerbUtility.CalculateAdjustedForcedMiss(verbProps.ForcedMissRadius,
                intendedIntVec - caster.Position);
            if (num > 0.5f)
            {
                var num2 = Rand.Range(0, GenRadial.NumCellsInRadius(num));
                if (num2 > 0) return (intendedIntVec + GenRadial.RadialPattern[num2]).ToVector3Shifted() + diffLost;
            }
        }

        return intended;
    }

    protected LocalTargetInfo GetTargetFromPos(IntVec3 pos, Verse.Map map)
    {
        var things = pos.GetThingList(map);
        if (things.NullOrEmpty()) return pos;
        return things.MaxBy(t => t.def.altitudeLayer);
    }

    public override float HighlightFieldRadiusAroundTarget(out bool needLOSToCenter)
    {
        needLOSToCenter = true;
        return ExplosionOnTargetSize;
    }
}