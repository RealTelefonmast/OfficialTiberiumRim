using UnityEngine;
using Verse;

namespace TR;

public class Subeffecter_SprayerFixed : SubEffecter
{
    public Subeffecter_SprayerFixed(SubEffecterDef def, Effecter parent) : base(def, parent)
    {
    }

    public override void SubTrigger(TargetInfo A, TargetInfo B, int overrideSpawnTick = -1)
    {
        MakeMote(A, B);
    }

    protected void MakeMote(TargetInfo A, TargetInfo B)
    {
        var vector = Vector3.zero;
        switch (def.spawnLocType)
        {
            case MoteSpawnLocType.OnSource:
                vector = A.CenterVector3;
                break;
            case MoteSpawnLocType.BetweenPositions:
            {
                var vector2 = A.HasThing ? A.Thing.DrawPos : A.Cell.ToVector3Shifted();
                var vector3 = B.HasThing ? B.Thing.DrawPos : B.Cell.ToVector3Shifted();
                if (A.HasThing && !A.Thing.Spawned)
                    vector = vector3;
                else if (B.HasThing && !B.Thing.Spawned)
                    vector = vector2;
                else
                    vector = vector2 * def.positionLerpFactor + vector3 * (1f - def.positionLerpFactor);
                break;
            }
            case MoteSpawnLocType.BetweenTouchingCells:
                vector = A.Cell.ToVector3Shifted() + (B.Cell - A.Cell).ToVector3().normalized * 0.5f;
                break;
            case MoteSpawnLocType.RandomCellOnTarget:
            {
                CellRect cellRect;
                if (B.HasThing)
                    cellRect = B.Thing.OccupiedRect();
                else
                    cellRect = CellRect.CenteredOn(B.Cell, 0);
                vector = cellRect.RandomCell.ToVector3Shifted();
                break;
            }
            case MoteSpawnLocType.OnTarget:
                vector = B.CenterVector3;
                break;
        }

        if (parent != null)
        {
            Rand.PushState(parent.GetHashCode());
            if (A.CenterVector3 != B.CenterVector3)
                vector += (B.CenterVector3 - A.CenterVector3).normalized * parent.def.offsetTowardsTarget.RandomInRange;
            vector += Gen.RandomHorizontalVector(parent.def.positionRadius) + parent.offset;
            Rand.PopState();
        }

        var map = A.Map ?? B.Map;
        float num;
        if (def.absoluteAngle)
            num = 0f;
        else if (def.useTargetAInitialRotation && A.HasThing)
            num = A.Thing.Rotation.AsAngle;
        else if (def.useTargetBInitialRotation && B.HasThing)
            num = B.Thing.Rotation.AsAngle;
        else
            num = (B.Cell - A.Cell).AngleFlat;
        var num2 = parent != null ? parent.scale : 1f;
        if (map != null && vector.ShouldSpawnMotesAt(map))
        {
            var randomInRange = def.burstCount.RandomInRange;
            for (var i = 0; i < randomInRange; i++)
            {
                var vector4 = vector + def.positionOffset * num2 +
                              Gen.RandomHorizontalVector(def.positionRadius) * num2;
                if (def.moteDef != null)
                {
                    var mote = (Mote)ThingMaker.MakeThing(def.moteDef);
                    GenSpawn.Spawn(mote, vector.ToIntVec3(), map);
                    mote.Scale = def.scale.RandomInRange * num2;
                    mote.exactPosition = vector4;
                    mote.rotationRate = def.rotationRate.RandomInRange;
                    mote.exactRotation = def.rotation.RandomInRange + num;
                    mote.instanceColor = EffectiveColor;
                    var moteThrown = mote as MoteThrown;
                    if (moteThrown != null)
                    {
                        moteThrown.airTimeLeft = def.airTime.RandomInRange;
                        moteThrown.SetVelocity(def.angle.RandomInRange + num, def.speed.RandomInRange);
                    }
                }
                else if (def.fleckDef != null)
                {
                    var velocityAngle = def.fleckUsesAngleForVelocity ? def.angle.RandomInRange + num : 0f;
                    map.flecks.CreateFleck(new FleckCreationData
                    {
                        def = def.fleckDef,
                        scale = def.scale.RandomInRange * num2,
                        spawnPosition = vector4,
                        rotationRate = def.rotationRate.RandomInRange,
                        rotation = def.rotation.RandomInRange + num,
                        instanceColor = EffectiveColor,
                        velocitySpeed = def.speed.RandomInRange,
                        velocityAngle = velocityAngle
                    });
                }
            }
        }
    }
}