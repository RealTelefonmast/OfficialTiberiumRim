using TeleCore.Events.Args;
using UnityEngine;
using Verse;

namespace TeleCore.Visual.VFX.Effecters;

/// <summary>
///     Holds and builds upon initial effect spawn data, providing necessary references down the line
/// </summary>
public struct ThrownEffectInfo
{
    public Vector3 origin;
    public Vector3 offset;
    public Vector3 final;
    public float scale;
    public float rotation;

    public float velocitySpeed;
    public float velocityAngle;

    public int overrideSpawnTick = -1;

    public Verse.Map map;
    public TargetInfo targetOrigin;
    public TargetInfo targetDestination;

    public ThrownEffectInfo(TargetInfo origin, TargetInfo destination)
    {
        targetOrigin = origin;
        targetDestination = destination;
        map = origin.Map ?? destination.Map;
    }

    public void AdjustOrigin(Vector3 valOffset, bool isSetter = false)
    {
        if (isSetter)
            origin = valOffset;
        else
            origin += valOffset;
    }

    public void AdjustOffset(Vector3 valOffset, bool isSetter = false)
    {
        if (isSetter)
            offset = valOffset;
        else
            offset += valOffset;
    }

    public void AdjustFinal(Vector3 valOffset, bool isSetter = false)
    {
        if (isSetter)
            final = valOffset;
        else
            final += valOffset;
    }

    //
    public void AdjustScale(float valOffset, bool isSetter = false)
    {
        if (isSetter)
            scale = valOffset;
        else
            scale += valOffset;
    }

    public void AdjustRotation(float valOffset, bool isSetter = false)
    {
        if (isSetter)
            rotation = valOffset;
        else
            rotation += valOffset;
    }

    //
    public void AdjustVelocitySpeed(float valOffset, bool isSetter = false)
    {
        if (isSetter)
            velocitySpeed = valOffset;
        else
            velocitySpeed += valOffset;
    }

    public void AdjustVelocityAngle(float valOffset, bool isSetter = false)
    {
        if (isSetter)
            velocityAngle = valOffset;
        else
            velocityAngle += valOffset;
    }


    public void AdjustSpawnTick(int spawnTick, bool isSetter = false)
    {
        if (isSetter)
            overrideSpawnTick = spawnTick;
        else
            overrideSpawnTick += spawnTick;
    }
}

public class SubEffecter_FX : SubEffecter
{
    private int burstCountLeft;

    //
    private Vector3? lastOffset;
    private int moteCount;
    private Mote sustainedMote;

    private int ticksLeft;
    private int ticksUntilBurst;

    //
    private int ticksUntilMote;

    public SubEffecter_FX(SubEffecterDef subDef, Effecter parent) : base(subDef, parent)
    {
    }

    public SubEffecterExtendedDef Def => (SubEffecterExtendedDef) def;

    public override void SubEffectTick(TargetInfo A, TargetInfo B)
    {
        switch (Def.effectMode)
        {
            case EffectThrowMode.Continuous:
                {
                    if (moteCount >= def.maxMoteCount) return;
                    ticksUntilMote--;
                    if (ticksUntilMote <= 0)
                    {
                        MakeMote(A, B);
                        ticksUntilMote = def.ticksBetweenMotes;
                        moteCount++;
                    }
                }
                break;
            case EffectThrowMode.ChancePerTick:
                {
                    var num = def.chancePerTick;
                    if (def.spawnLocType == MoteSpawnLocType.RandomCellOnTarget && B.HasThing)
                        num *= B.Thing.def.size.x * B.Thing.def.size.z;
                    if (Rand.Value < num)
                        MakeMote(A, B);
                }
                break;
            case EffectThrowMode.Burst:
                {
                    if (Def.burstInterval.Average > 0)
                    {
                        if (ticksUntilBurst > 0)
                        {
                            ticksUntilBurst--;
                        }
                        else if (burstCountLeft > 0)
                        {
                            burstCountLeft--;
                            MakeMote(A, B);
                        }
                        else
                        {
                            ticksUntilBurst = Def.burstInterval.RandomInRange;
                            burstCountLeft = Def.burstCount.RandomInRange;
                        }
                    }
                    else
                    {
                        ticksLeft--;
                        if (ticksLeft <= 0)
                        {
                            MakeMote(A, B);
                            ticksLeft = Def.throwInterval.RandomInRange;
                        }
                    }
                }
                break;
        }
    }

    public override void SubTrigger(TargetInfo A, TargetInfo B, int overrideSpawnTick = -1, bool force = false)
    {
        MakeMote(A, B, overrideSpawnTick);
    }

    public override void SubCleanup()
    {
        base.SubCleanup();
    }

    //Effect Creation
    protected void MakeMote(TargetInfo A, TargetInfo B, int overrideSpawnTick = -1)
    {
        //
        var map = A.Map ?? B.Map;
        if (map == null) return;

        //
        var info = new ThrownEffectInfo(A, B);
        info.AdjustSpawnTick(overrideSpawnTick, true);

        //Set Origin Pos
        switch (def.spawnLocType)
        {
            case MoteSpawnLocType.OnSource:
                info.AdjustOrigin(A.CenterVector3, true);
                break;
            case MoteSpawnLocType.OnTarget:
                info.AdjustOrigin(B.CenterVector3, true);
                break;
            case MoteSpawnLocType.BetweenPositions:
            {
                var vector2 = A.HasThing ? A.Thing.DrawPos : A.Cell.ToVector3Shifted();
                var vector3 = B.HasThing ? B.Thing.DrawPos : B.Cell.ToVector3Shifted();
                if (A.HasThing && !A.Thing.Spawned)
                    info.AdjustOrigin(vector3, true);
                else if (B.HasThing && !B.Thing.Spawned)
                    info.AdjustOrigin(vector2, true);
                else
                    info.AdjustOrigin(vector2 * def.positionLerpFactor + vector3 * (1f - def.positionLerpFactor), true);
                break;
            }
            case MoteSpawnLocType.BetweenTouchingCells:
                info.AdjustOrigin(A.Cell.ToVector3Shifted() + (B.Cell - A.Cell).ToVector3().normalized * 0.5f, true);
                break;
            case MoteSpawnLocType.RandomCellOnTarget:
            {
                var cellRect = B.HasThing ? B.Thing.OccupiedRect() : CellRect.CenteredOn(B.Cell, 0);
                info.AdjustOrigin(cellRect.RandomCell.ToVector3Shifted(), true);
                break;
            }
            case MoteSpawnLocType.RandomDrawPosOnTarget:
                if (B.Thing.DrawSize != Vector2.one && B.Thing.DrawSize != Vector2.zero)
                {
                    var vector4 = B.Thing.DrawSize.RotatedBy(B.Thing.Rotation);
                    var b = new Vector3(vector4.x * Rand.Value, 0f, vector4.y * Rand.Value);
                    info.AdjustOrigin(B.CenterVector3 + b - new Vector3(vector4.x / 2f, 0f, vector4.y / 2f), true);
                }
                else
                {
                    var b2 = new Vector3(Rand.Value, 0f, Rand.Value);
                    info.AdjustOrigin(B.CenterVector3 + b2 - new Vector3(0.5f, 0f, 0.5f), true);
                }

                break;
        }

        //
        if (parent != null)
        {
            Rand.PushState(parent.GetHashCode());
            if (A.CenterVector3 != B.CenterVector3)
                info.AdjustOrigin((B.CenterVector3 - A.CenterVector3).normalized *
                                  parent.def.offsetTowardsTarget.RandomInRange);
            var a = Gen.RandomHorizontalVector(parent.def.positionRadius);
            Rand.PopState();

            if (def.positionDimensions != null) a += Gen.Random2DVector(def.positionDimensions.Value);
            info.AdjustOrigin(a + parent.offset);
        }

        //Set Rotation
        if (def.absoluteAngle)
            info.AdjustRotation(0, true);
        else if (def.useTargetAInitialRotation && A.HasThing)
            info.AdjustRotation(A.Thing.Rotation.AsAngle, true);
        else if (def.useTargetBInitialRotation && B.HasThing)
            info.AdjustRotation(B.Thing.Rotation.AsAngle, true);
        else
            info.AdjustRotation((B.Cell - A.Cell).AngleFlat, true);

        //Set Scale
        info.AdjustScale(parent?.scale ?? 1f, true);

        //Set Velocity
        var speed = def.speed.RandomInRange;
        var angle = def.angle.RandomInRange;

        if (Def.affectedByWind)
        {
            var pos = A.HasThing ? A.Thing.Position : A.Cell;
            var windSpeed = pos.GetRoom(map).PsychologicallyOutdoors ? map.windManager.WindSpeed : 0f;
            var windPct = Mathf.InverseLerp(0f, 2f, windSpeed);
            speed *= Mathf.Lerp(0.1f, 1, windPct);
            angle = (int) Mathf.Lerp(def.angle.min, def.angle.max, windPct);
        }

        info.AdjustVelocityAngle(angle, true);
        info.AdjustVelocitySpeed(speed, true);

        //Do Burst
        var randomInRange = def.burstCount.RandomInRange;
        for (var i = 0; i < randomInRange; i++)
        {
            info.AdjustOffset(def.positionOffset, true);
            if (def.useTargetAInitialRotation && A.HasThing)
                info.AdjustOffset(info.offset.RotatedBy(A.Thing.Rotation), true);
            else if (def.useTargetBInitialRotation && B.HasThing)
                info.AdjustOffset(info.offset.RotatedBy(B.Thing.Rotation), true);

            if (!def.perRotationOffsets.NullOrEmpty())
                info.AdjustOffset(def.perRotationOffsets[
                    (def.spawnLocType == MoteSpawnLocType.OnSource ? A.Thing.Rotation : B.Thing.Rotation).AsInt]);

            for (var j = 0; j < 5; j++)
            {
                info.AdjustOffset(
                    info.offset * info.scale + Rand.InsideAnnulusVector3(def.positionRadiusMin, def.positionRadius) *
                    info.scale, true);
                if (def.avoidLastPositionRadius < 1E-45f || lastOffset == null ||
                    (info.offset - lastOffset.Value).MagnitudeHorizontal() > def.avoidLastPositionRadius)
                    break;
            }

            lastOffset = info.offset;
            info.AdjustFinal(info.origin + info.offset, true);
            if (def.rotateTowardsTargetCenter && B.HasThing)
                info.AdjustRotation((info.final - B.CenterVector3).AngleFlat(), true);

            //
            var thing = A.Thing ?? B.Thing;
            if (Def.originOffsets != null && thing != null)
            {
                var final = info.final;
                foreach (var originOffset in Def.originOffsets[thing.Rotation])
                {
                    info.AdjustFinal(final + originOffset, true);
                    TrySpawnMote(info);
                    TrySpawnFleck(info);
                }
            }
            else
            {
                TrySpawnMote(info);
                TrySpawnFleck(info);
            }
        }
    }

    private void TrySpawnFleck(ThrownEffectInfo info)
    {
        var effectDef = def.fleckDef;
        if (effectDef == null) return;
        if (info.final.ShouldSpawnMotesAt(info.map, effectDef.drawOffscreen))
        {
            info.map.flecks.CreateFleck(new FleckCreationData
            {
                def = effectDef,
                scale = def.scale.RandomInRange * info.scale,
                spawnPosition = info.final,
                rotationRate = def.rotationRate.RandomInRange,
                rotation = def.rotation.RandomInRange + info.rotation,
                instanceColor = EffectiveColor,
                velocitySpeed = info.velocitySpeed,
                velocityAngle = info.velocityAngle,
                ageTicksOverride = info.overrideSpawnTick
            });

            //Try Notify Parent
            if (Def?.eventTag != null)
                ((Effecter_FX) parent)?.SpawnedEffect(new FXEffecterSpawnedEventArgs
                {
                    //Cant pass fleck on as there is no return value when creating
                    effecterTag = Def.eventTag,
                    fleckDef = effectDef
                });
        }
    }

    //
    private void TrySpawnMote(ThrownEffectInfo info)
    {
        if (def.moteDef == null) return;
        if (info.final.ShouldSpawnMotesAt(info.map, def.moteDef.drawOffscreen))
        {
            sustainedMote = (Mote) ThingMaker.MakeThing(def.moteDef);
            GenSpawn.Spawn(sustainedMote, info.final.ToIntVec3(), info.map);
            sustainedMote.Scale = def.scale.RandomInRange * info.scale;
            sustainedMote.exactPosition = info.final;
            sustainedMote.rotationRate = def.rotationRate.RandomInRange;
            sustainedMote.exactRotation = def.rotation.RandomInRange + info.rotation;
            sustainedMote.instanceColor = EffectiveColor;

            //
            if (info.overrideSpawnTick != -1)
                sustainedMote.ForceSpawnTick(info.overrideSpawnTick);

            //
            if (sustainedMote is MoteThrown moteThrown)
            {
                moteThrown.airTimeLeft = def.airTime.RandomInRange;
                moteThrown.SetVelocity(info.velocityAngle, info.velocitySpeed);
            }

            //
            if (def.attachToSpawnThing)
                if (sustainedMote is MoteAttached moteAttached)
                {
                    if (def.spawnLocType == MoteSpawnLocType.OnSource && info.targetOrigin.HasThing)
                        moteAttached.Attach(info.targetOrigin, info.offset);
                    else if (def.spawnLocType == MoteSpawnLocType.OnTarget && info.targetDestination.HasThing)
                        moteAttached.Attach(info.targetDestination, info.offset);
                }

            sustainedMote.Maintain();

            //Try Notify Parent
            if (Def?.eventTag != null)
                ((Effecter_FX) parent)?.SpawnedEffect(new FXEffecterSpawnedEventArgs
                {
                    effecterTag = Def.eventTag,
                    mote = sustainedMote
                });
        }
    }
}