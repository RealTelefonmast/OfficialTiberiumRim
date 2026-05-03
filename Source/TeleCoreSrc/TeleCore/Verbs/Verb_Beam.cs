using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace TeleCore.Unsorted;

public class Verb_Beam : Verb_Tele
{
    //Angle
    private float aimAnglDamper;
    private List<Vector3> beamHitLocations = new();

    private Vector3? currentAimOverrideTarget;

    //Target
    private Vector3 currentTargetTruePos;

    //Effects
    private MoteBeam movingBeamMote;
    private Effecter movingEndEffecter;
    private float rotationSpeed;
    private Sustainer sustainer;
    private int ticksToNextLocation;

    //
    public BeamProperties BeamProps => Props.beamProps;
    public bool IsStatic => BeamProps.isStatic;

    //
    public override DamageDef DamageDef => BeamProps.damageDef ?? verbProps.beamDamageDef;
    protected override float ExplosionOnTargetSize => BeamProps.impactExplosion?.explosionRadius ?? 0;

    //Beam Moving Mechanic
    private float ShotProgress => ticksToNextLocation / (float)verbProps.ticksBetweenBurstShots;

    private Vector3 InterpolatedPosition
    {
        get
        {
            if (IsStatic) return beamHitLocations[burstShotsLeft];
            return Vector3.Lerp(beamHitLocations[burstShotsLeft],
                       beamHitLocations[Mathf.Min(burstShotsLeft + 1, beamHitLocations.Count - 1)], ShotProgress) +
                   (CurrentTarget.CenterVector3 - currentTargetTruePos);
        }
    }

    public override float? AimAngleOverride
    {
        get
        {
            if (state != VerbState.Bursting && currentAimOverrideTarget == null)
                return DesiredAimAngle;
            return aimAnglDamper;
        }
    }

    private void SetAimAngle(Vector3 newAimTarget)
    {
        currentAimOverrideTarget = newAimTarget;
        aimAnglDamper = (currentAimOverrideTarget - CurrentStartPos).Value.AngleFlat();
        turretGun?.Top?.Notify_AimAngleChanged(AimAngleOverride);
    }

    public override void PostVerbTick()
    {
        if (currentAimOverrideTarget != null)
        {
            if (state != VerbState.Bursting)
                aimAnglDamper = Mathf.SmoothDampAngle(aimAnglDamper, DesiredAimAngle, ref rotationSpeed, 0.01f, 8,
                    0.01666f);
            if (TMath.Abs(aimAnglDamper - DesiredAimAngle) < 0.015625f) currentAimOverrideTarget = null;
        }
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Collections.Look(ref beamHitLocations, "path", LookMode.Value);
        Scribe_Values.Look(ref ticksToNextLocation, "ticksToNextPathStep");
        Scribe_Values.Look(ref currentTargetTruePos, "initialTargetPosition");
        if (Scribe.mode == LoadSaveMode.PostLoadInit && beamHitLocations == null)
            beamHitLocations = new List<Vector3>();
    }

    protected override bool IsAvailable()
    {
        return IsBeam;
    }

    private MoteBeam MakeBeamMote(IntVec3 targetLoc)
    {
        return TMoteMaker.MakeBeamEffect(verbProps.beamMoteDef, caster, new TargetInfo(targetLoc, caster.Map),
            BeamProps.VisualWidthRange.RandomInRange);
    }

    //Prepare Beam Hit Target Cells
    public override void WarmupComplete()
    {
        beamHitLocations.Clear();
        burstShotsLeft = ShotsPerBurst;
        state = VerbState.Bursting;
        currentTargetTruePos = currentTarget.CenterVector3.Yto0();

        //
        var rangeDiff = currentTargetTruePos - CurrentStartPos.Yto0();
        var magnitude = rangeDiff.magnitude;
        var normalized = rangeDiff.normalized;

        //
        if (IsStatic)
            SetupStaticTargets();
        else
            SetupBeamLineTargets(normalized, magnitude);

        TryCastNextBurstShot();

        ticksToNextLocation = verbProps.ticksBetweenBurstShots;
        movingEndEffecter?.Cleanup();
        if (verbProps.soundCastBeam != null)
            sustainer = verbProps.soundCastBeam.TrySpawnSustainer(SoundInfo.InMap(caster, MaintenanceType.PerTick));
    }

    private void SetupBeamLineTargets(Vector3 normalVec, float magnitude)
    {
        var normalRotated = normalVec.RotatedBy(-90f);
        var fullWidthRangeFactor = verbProps.beamFullWidthRange > 0f
            ? Mathf.Min(magnitude / verbProps.beamFullWidthRange, 1f)
            : 1f;
        var beamWidthStep = (verbProps.beamWidth + 1f) * fullWidthRangeFactor / ShotsPerBurst;
        var finalMovingBeamTarget =
            currentTargetTruePos - normalRotated * verbProps.beamWidth / 2f * fullWidthRangeFactor;
        var a2 = normalVec * (Rand.Value * verbProps.beamMaxDeviation) - normalVec / 2f;

        //
        for (var i = 0; i <= ShotsPerBurst; i++)
        {
            //
            var b = Mathf.Sin((i / (float)ShotsPerBurst + 0.5f) * Mathf.PI * Mathf.Rad2Deg) * verbProps.beamCurvature *
                -normalVec - normalVec * verbProps.beamMaxDeviation / 2f;
            beamHitLocations.Add(finalMovingBeamTarget + (a2 + b) * fullWidthRangeFactor);
            finalMovingBeamTarget += normalRotated * beamWidthStep;
        }

        //Get Beam Instance
        if (verbProps.beamMoteDef != null && !BeamProps.spawnMotePerBeam)
            movingBeamMote = MakeBeamMote(beamHitLocations[0].ToIntVec3());

        //
        if (movingEndEffecter == null && verbProps.beamEndEffecterDef != null)
            movingEndEffecter = verbProps.beamEndEffecterDef.Spawn(caster.Position, caster.Map);
    }

    private void SetupStaticTargets()
    {
        for (var i = 0; i <= ShotsPerBurst; i++)
        {
            //
            _ = TryFindShootLineFromTo(caster.Position, currentTarget, out var shootLine);
            var shotReport = ShotReport.HitReportFor(caster, this, currentTarget);
            var randomCoverToMissInto = shotReport.GetRandomCoverToMissInto();
            var finalStaticShotTarget = currentTargetTruePos;
            if (verbProps.canGoWild && !Rand.Chance(shotReport.AimOnTargetChance_IgnoringPosture))
            {
                shootLine.ChangeDestToMissWild(shotReport.AimOnTargetChance_StandardTarget, caster.Map);
                finalStaticShotTarget = shootLine.Dest.ToVector3Shifted().Yto0();
            }

            if (currentTarget.Thing != null && currentTarget.Thing.def.CanBenefitFromCover &&
                !Rand.Chance(shotReport.PassCoverChance))
                finalStaticShotTarget = randomCoverToMissInto.TrueCenter().Yto0();
            beamHitLocations.Add(finalStaticShotTarget);
        }
    }

    //
    public override void BurstingTick()
    {
        //Enter next step
        ticksToNextLocation--;

        if (IsStatic) return;
        var curPos = InterpolatedPosition;
        var curPosIntVec = curPos.ToIntVec3();
        var curRangeDiff = InterpolatedPosition - CurrentStartPos;
        var normalized = curRangeDiff.Yto0().normalized;
        var magnitude = curRangeDiff.MagnitudeHorizontal();
        var LOSInterjection = CellGen.LastPointOnLineOfSightWithHeight(ShotOriginLOS, curPos.Yto0(), Props.minHitHeight,
            c => c.CanBeSeenOverFast(caster.Map), true);

        //Adjust Target By LOS Interjection
        if (LOSInterjection.IsValid)
        {
            //Have valid target pos
            magnitude -= (curPosIntVec - LOSInterjection).LengthHorizontal;
            curPos = CurrentStartPos + normalized * magnitude;
            curPosIntVec = curPos.ToIntVec3();
        }

        //
        BurstTickEffects(curPos, curPosIntVec, normalized, magnitude);
    }

    private void BurstTickEffects(Vector3 targetVec3, IntVec3 targetIntVec, Vector3 normalVec, float magnitude)
    {
        var targetCellToRealOffset = targetVec3 - targetIntVec.ToVector3Shifted();
        var startRealOffset = normalVec * verbProps.beamStartOffset + CurrentDrawOffset;

        //Spawn Fleck On Ground
        if (verbProps.beamGroundFleckDef != null && Rand.Chance(verbProps.beamFleckChancePerTick))
            FleckMaker.Static(targetVec3, caster.Map, verbProps.beamGroundFleckDef);

        //Draw Moving Beam Mote
        if (movingBeamMote != null)
        {
            movingBeamMote.UpdateTargets(new TargetInfo(caster.Position, caster.Map),
                new TargetInfo(targetIntVec, caster.Map), startRealOffset, targetCellToRealOffset);
            movingBeamMote.Maintain();
            SetAimAngle(targetVec3);
        }

        //Do Moving Endeffecter
        if (movingEndEffecter != null)
        {
            movingEndEffecter.offset = targetCellToRealOffset;
            movingEndEffecter.EffectTick(new TargetInfo(targetIntVec, caster.Map), TargetInfo.Invalid);
            movingEndEffecter.ticksLeft--;
        }

        //
        SpawnFlecksOnBeam(CurrentStartPos, normalVec, magnitude);

        //Do Sound Sustainer
        sustainer?.Maintain();
    }

    private void SpawnFlecksOnBeam(Vector3 startPos, Vector3 normalVec, float magnitude)
    {
        //Spawn Effects along beam line
        if (verbProps.beamLineFleckDef != null)
        {
            var targetRange = magnitude;
            var curRange = 0;
            while (curRange < targetRange)
            {
                if (Rand.Chance(verbProps.beamLineFleckChanceCurve.Evaluate(curRange / targetRange)))
                {
                    var curPos = curRange * normalVec;
                    var randPos = normalVec * Rand.Value;
                    var b2 = curPos - randPos + normalVec * 0.5f;
                    FleckMaker.Static(startPos + b2, caster.Map, verbProps.beamLineFleckDef);
                }

                curRange++;
            }
        }
    }

    private void
        StaticTargetEffects(Vector3 startPos, Vector3 staticVec3,
            IntVec3 staticTarget) //, Vector3 rangeDiff, Vector3 normalVec
    {
        var rangeDiff = staticVec3 - startPos;
        var normalVec = rangeDiff.Yto0().normalized;

        var targetCellToRealOffset = staticVec3 - staticTarget.ToVector3Shifted();
        var curOff = CurrentDrawOffset;
        var startRealOffset = curOff + normalVec * verbProps.beamStartOffset;

        //Setup static mote on target
        if (BeamProps.spawnMotePerBeam)
        {
            TLog.Message($"Index[{OffsetIndex}]: {curOff} => {startRealOffset}");
            var newBeam = MakeBeamMote(staticTarget);
            newBeam.UpdateTargets(new TargetInfo(caster.Position, caster.Map), new TargetInfo(staticTarget, caster.Map),
                startRealOffset, targetCellToRealOffset);
            newBeam.Maintain();
            SetAimAngle(staticVec3);
        }

        if (verbProps.beamLineFleckDef != null)
        {
            var normalized = normalVec;
            var num2 = 1f * rangeDiff.MagnitudeHorizontal();
            var num3 = 0;
            while (num3 < num2)
            {
                for (var k = 0; k < 4; k++)
                    if (Rand.Chance(verbProps.beamLineFleckChanceCurve.Evaluate(num3 / num2)))
                    {
                        var b2 = num3 * normalized - normalized * Rand.Value + normalized / 2f;
                        FleckMaker.Static(caster.Position.ToVector3Shifted() + b2, caster.Map,
                            verbProps.beamLineFleckDef);
                    }

                num3++;
            }
        }

        /*
        if (verbProps.burstShotCount <= 1)
        {
            if (movingBeamMote != null)
            {
                movingBeamMote.UpdateTargets(new TargetInfo(caster.Position, caster.Map), new TargetInfo(staticTarget, caster.Map), (CurrentRangeDiffNormalized * verbProps.beamStartOffset) + GraphicalShotOffset, desiredVec - finalPos.ToVector3Shifted());
                movingBeamMote.Maintain();
            }

            if (verbProps.beamLineFleckDef != null)
            {
                var normalized = CurrentRangeDiffNormalized;
                var num2 = 1f * CurrentRangeDiff.MagnitudeHorizontal();
                var num3 = 0;
                while (num3 < num2)
                {
                    for (int k = 0; k < 4; k++)
                    {
                        if (Rand.Chance(verbProps.beamLineFleckChanceCurve.Evaluate(num3 / num2)))
                        {
                            var b2 = num3 * normalized - normalized * Rand.Value + normalized / 2f;
                            FleckMaker.Static(caster.Position.ToVector3Shifted() + b2, caster.Map, verbProps.beamLineFleckDef);
                        }
                    }
                    num3++;
                }
            }
        }
        */
    }

    protected override bool TryCastAttack()
    {
        if (currentTarget.HasThing && currentTarget.Thing.Map != caster.Map) return false;
        if (verbProps.stopBurstWithoutLos && !TryFindShootLineFromTo(caster.Position, currentTarget, out _))
            return false;

        lastShotTick = Find.TickManager.TicksGame;
        ticksToNextLocation = verbProps.ticksBetweenBurstShots;
        var desiredVec = InterpolatedPosition.Yto0();
        var desiredRange = desiredVec.ToIntVec3();
        var rangePos = CellGen.LastPointOnLineOfSightWithHeight(ShotOriginLOS, desiredVec, 1,
            c => c.CanBeSeenOverFast(caster.Map), true);

        var finalPos = rangePos.IsValid ? rangePos : desiredRange;

        //
        if (IsStatic) StaticTargetEffects(CurrentStartPos, finalPos.ToVector3Shifted(), finalPos);

        HitCell(finalPos);
        return true;
    }

    //Hit Logic
    private bool CanHit(Thing thing)
    {
        return thing.Spawned && !CoverUtility.ThingCovered(thing, caster.Map);
    }

    private void HitCell(IntVec3 cell)
    {
        //Do impact effects
        Props.beamProps.impactEffecter?.Spawn(cell, caster.Map);
        Props.beamProps.impactExplosion?.DoExplosion(cell, caster.Map, Caster);
        Props.beamProps.impactFilth?.SpawnFilth(cell, caster.Map);
        ApplyDamage(VerbUtility.ThingsToHit(cell, caster.Map, CanHit).RandomElementWithFallback());
    }

    private void ApplyDamage(Thing thing)
    {
        if (thing == null || DamageDef == null) return;

        var intVec = thing.Position;
        var intVec2 = CellGen.LastPointOnLineOfSightWithHeight(ShotOriginLOS, intVec.ToVector3(), 1,
            c => c.CanBeSeenOverFast(caster.Map), true);
        if (intVec2.IsValid) intVec = intVec2;

        var angleFlat = (currentTarget.Cell - caster.Position).AngleFlat;
        var log = new BattleLogEntry_RangedImpact(EquipmentSource, thing, currentTarget.Thing, EquipmentSource.def,
            null, null);

        var damageBase = BeamProps.damageBaseOverride ?? DamageDef.defaultDamage;
        var armorPen = BeamProps.armorPenetrationOverride ?? DamageDef.defaultArmorPenetration;
        var stoppingPower = BeamProps.stoppingPowerOverride ?? DamageDef.defaultStoppingPower;

        var dinfo = new DamageInfo(DamageDef, damageBase, armorPen, angleFlat, caster, null, GunDef,
            DamageInfo.SourceCategory.ThingOrUnknown, currentTarget.Thing);
        thing.TakeDamage(dinfo).AssociateWithLog(log);

        var hitPawn = thing as Pawn;
        if (hitPawn?.stances != null && hitPawn.BodySize <= stoppingPower + 0.001f)
            hitPawn.stances.stagger.StaggerFor(BeamProps.staggerTime.SecondsToTicks());

        if (thing.CanEverAttachFire())
        {
            if (Rand.Chance(verbProps.beamChanceToAttachFire))
                thing.TryAttachFire(verbProps.beamFireSizeRange.RandomInRange, caster);
        }
        else if (Rand.Chance(verbProps.beamChanceToStartFire))
        {
            FireUtility.TryStartFireIn(intVec, caster.Map, verbProps.beamFireSizeRange.RandomInRange, caster);
        }
    }

    protected override BattleLogEntry_RangedFire EntryOnWarmupComplete()
    {
        return new BattleLogEntry_RangedFire(caster, currentTarget.HasThing ? currentTarget.Thing : null,
            EquipmentSource != null ? EquipmentSource.def : null, null, ShotsPerBurst > 1);
    }
}