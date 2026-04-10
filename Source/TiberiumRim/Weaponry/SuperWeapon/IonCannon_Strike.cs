using System;
using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using TeleCore.ActionCompositions;
using TR.Effects;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace TR.SuperWeapon;

public class IonCannon_Strike : ThingWithComps
{
    public static readonly float radius = 34f;
    private static readonly float degrees = 360;
    private static readonly int beamCount = 8;
    private readonly List<IonBeam> beamList = new();
    private readonly List<IntVec3> beamPositions = new();
    private int curBeamCount;

    public float curDegree;

    private float curRadius;

    public float radiusPerDegree;

    //
    public AttackSatellite_Ion satellite;

    //Stage One -- Setup
    public int ticksForSetup;

    //Stage Two -- Spiral
    public int ticksForSpiral;

    public int ticksPerBeam;
    public int ticksPerDegree;
    private float totalSeconds = 12;

    public int TotalTime => ticksForSetup + ticksForSpiral;
    public float TickMultiplier => degrees / ticksForSpiral;

    //Stage Three -- Climax
    public override void ExposeData()
    {
        base.ExposeData();
    }

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
        if (!respawningAfterLoad)
        {
            //Setup
            ticksForSetup = GenTicks.SecondsToTicks(3);
            ticksPerBeam = ticksForSetup / (beamCount - 1);
            curRadius = radius;

            //Spiral
            ticksForSpiral = GenTicks.SecondsToTicks(5);
            radiusPerDegree = radius / degrees;
            ticksPerDegree = ticksForSpiral / (int)degrees;

            //
            SetupPositions();
            Log.Message("## Setting Up Ion Cannon ##");
            Log.Message("Radius: " + radius + " RadiusDownGrade: " + radiusPerDegree);
            Log.Message("Spiral Ticks: " + ticksForSpiral + " ticks per degree: " + ticksPerDegree);
            Log.Message("Setup Ticks: " + ticksForSetup + " ticks per beam: " + ticksPerBeam);

            var posInfo = new TargetInfo(Position, map);
            var composition = new ActionComposition("Ion Cannon Strike");
            composition.CacheMap(new GlobalTargetInfo(Position, Map));
            composition.AddPart(delegate
            {
                //DoAscensionParticlesInRadius(Position, 8, 6, new IntRange(20, 30));
                TiberiumFX.StartDustEffecter(Position, Map, radius, 12);
            }, 0);
            composition.AddPart(delegate { DoSetup(composition.CurrentTick); }, SoundDef.Named("IonCannon_StartUp"),
                SoundInfo.InMap(posInfo, MaintenanceType.PerFrame), 0f, 3f);
            composition.AddPart(delegate { DoSpiral(composition.CurrentTick); },
                SoundDef.Named("IonCannon_InitialCharge"), SoundInfo.InMap(posInfo, MaintenanceType.PerFrame), 3f, 5f);
            composition.AddPart(delegate
            {
                MakeIonBubble(5, radius, new ColorInt(70, 90, 175).ToColor, ThingDef.Named("IonDistortionBubble"));
                MakeIonBubble(5, radius * 0.4f, new ColorInt(100, 120, 175).ToColor,
                    ThingDef.Named("IonCenterDistortionBubble"));
            }, 7f); // 5 second alive time for bubble
            composition.AddPart(SoundDef.Named("IonCannon_PreClimaxPause"),
                SoundInfo.InMap(posInfo, MaintenanceType.PerFrame), 8f);
            composition.AddPart(SoundDef.Named("IonCannon_ClimaxChargeUp"),
                SoundInfo.InMap(posInfo, MaintenanceType.PerFrame), 10f); // 10.25f);
            composition.AddPart(delegate
            {
                //TODO: Notify Satellite; Draw Beam on world 
            }, 11f);
            composition.AddPart(delegate
            {
                var beam = (IonBeam)ThingMaker.MakeThing(ThingDef.Named("IonBeam"));
                beam.realPos = DrawPos;
                beam.durationTicks = 60;
                beam.width = 8;
                beam.continuousBurn = false;
                GenSpawn.Spawn(beam, Position, Map);
                //GenExplosion.DoExplosion(Position, Map, radius, DamageDefOf.Bomb, this, TRUtils.Range(1000, 9999));

                //DO DETONATION
                /*
                LongEventHandler.QueueLongEvent( delegate
                {
                    Log.Message("Doing long event");
                    foreach (var cell in Map.AllCells)
                    {
                        var list = cell.GetThingList(Map);
                        for (var i = list.Count - 1; i >= 0; i--)
                        {
                            var thing = list[i];
                            var pawn = thing as Pawn;
                            if (thing == null) continue;
                            if (!thing.def.destroyable || !thing.def.useHitPoints) continue;
                            float damage = pawn != null ? 1000 : (Rand.Range(thing.MaxHitPoints * 0.45f, thing.MaxHitPoints) * 2);
                            var dinfo = new DamageInfo(DamageDefOf.Flame, damage, 100);
                            thing.TakeDamage(dinfo);
                            if (thing.Destroyed)
                                GenSpawn.Spawn(ThingDefOf.Filth_Ash, cell, Map);
                            if(thing.def.IsBuilding() && Rand.Chance(0.5f))
                                GenSpawn.Spawn(ThingDefOf.Filth_RubbleRock, cell, Map);
                            if (pawn?.Destroyed ?? false)
                            {
                                GenSpawn.Spawn(ThingDefOf.Filth_CorpseBile, cell, Map);
                            }
                        }
                    }
                }, "Ion Cannon Strike", true, null);
                */
                foreach (var intVec3 in GenRadial.RadialCellsAround(Position, radius, true))
                {
                    var list = intVec3.GetThingList(Map);
                    for (var i = list.Count - 1; i >= 0; i--)
                    {
                        var thing = list[i];
                        var pawn = thing as Pawn;
                        if (thing == null) continue;
                        if ((!(thing is Pawn) && !thing.def.destroyable) || !thing.def.useHitPoints) continue;
                        var damage = Mathf.Lerp(3000, 100, Position.DistanceTo(intVec3) / radius);
                        var dinfo = new DamageInfo(DamageDefOf.Flame, damage, 100);
                        thing.TakeDamage(dinfo);
                    }
                }

                var mote = (Mote)ThingMaker.MakeThing(ThingDef.Named("IonBurnMark"));
                mote.exactPosition = DrawPos;
                mote.Scale = radius * 6.5f;
                mote.rotationRate = 1.2f;
                GenSpawn.Spawn(mote, Position, Map);
                //MakeIonBubble(2, 8, new ColorInt(120, 140, 225).ToColor, ThingDef.Named("IonCenterDistortionBubble"));
                //MoteMaker.MakeStaticMote(Position, Map, ThingDef.Named("IonExplosionShockwave"), radius);

                var ionExpComp = new ActionComposition("Ion Last Exp ");
                var distortion = (Mote)ThingMaker.MakeThing(ThingDef.Named("IonExplosionShockwave"));
                ionExpComp.AddPart(delegate
                {
                    distortion.exactPosition = DrawPos;
                    distortion.rotationRate = 1.2f;
                    GenSpawn.Spawn(distortion, Position, Map);
                }, 0);
                ionExpComp.AddPart(
                    delegate(ActionPart part)
                    {
                        distortion.Scale = radius * (part.CurrentTick / (float)part.playTime);
                    }, 0, 0.75f);
                ionExpComp.Init();
            }, SoundDef.Named("IonCannon_Climax"), SoundInfo.InMap(posInfo, MaintenanceType.PerFrame), 12f);
            composition.AddFinishAction(delegate
            {
                Log.Message("Ion Cannon Should Be Destroyed Now");
                TiberiumFX.DoAscensionParticlesInRadius(composition.target.Cell, composition.target.Map, radius / 4, 16,
                    new IntRange(10, 20));
                TiberiumFX.DoFloatingEffectsInRadius(composition.target.Cell, composition.target.Map, radius / 4, 20,
                    false, new IntRange(20, 30), new IntRange(6, 10), new IntRange(1, 5));
                TiberiumFX.DoFloatingEffectsInRadius(composition.target.Cell, composition.target.Map, radius, 20, true,
                    new IntRange(5, 15), new IntRange(1, 6), IntRange.one);
                Destroy();
            });
            composition.Init();
        }
    }

    //TODO: Make new Mote class, control fade-in and fade-out times directly, instead over mote properties 
    //TODO: Make new mote overridable, no more static mote tiberium.
    //
    //

    private void MakeIonBubble(float time, float bubbleRadius, Color color, ThingDef ionDistortion)
    {
        var composition = new ActionComposition("Ion Bübble " + ionDistortion);
        var mote = (Mote)ThingMaker.MakeThing(ThingDef.Named("IonBubble"));
        var distortion = (Mote)ThingMaker.MakeThing(ionDistortion);
        composition.AddPart(delegate
        {
            mote.exactPosition = distortion.exactPosition = DrawPos;
            mote.Scale = bubbleRadius;
            mote.rotationRate = distortion.rotationRate = 1.2f;
            mote.instanceColor = color;
            GenSpawn.Spawn(mote, Position, Map);
            GenSpawn.Spawn(distortion, Position, Map);
        }, 0);
        composition.AddPart(
            delegate(ActionPart part)
            {
                distortion.Scale = mote.Scale = bubbleRadius * (part.CurrentTick / (float)part.playTime);
            }, 0, time);
        composition.Init();
    }

    private void SetupPositions()
    {
        var origin = DrawPos;
        float x;
        float z;
        while (beamPositions.Count < 8)
        {
            var d = beamPositions.Count * degrees / beamCount * (Math.PI / 180);
            x = (float)(origin.x + curRadius * Math.Cos(d));
            z = (float)(origin.z + curRadius * Math.Sin(d));
            beamPositions.Add(new IntVec3((int)x, 0, (int)z));
        }
    }

    private void DoSetup(int tick)
    {
        if (tick % ticksPerBeam == 0 && curBeamCount < beamCount)
        {
            var pos = beamPositions.RandomElement();
            beamPositions.Remove(pos);
            if (pos.InBounds(Map))
            {
                var beam = (IonBeam)ThingMaker.MakeThing(ThingDef.Named("IonBeam"));
                beam.realPos = pos.ToVector3Shifted();
                beam.durationTicks = TotalTime - tick + GenTicks.SecondsToTicks(1);
                GenSpawn.Spawn(beam, pos, Map);
                SoundDef.Named("IonCannon_Laser" + (curBeamCount + 1)).PlayOneShot(SoundInfo.InMap(beam));
                beamList.Add(beam);
            }

            //Lets shake the camera a bit
            Find.CameraDriver.shaker.SetMinShake(50);
            curBeamCount++;
        }
    }

    private void DoSpiral(int curTick)
    {
        var origin = Position.ToVector3Shifted();
        for (var i = 0; i < beamList.Count; i++)
        {
            float x;
            float z;
            var d = (curDegree + (i - 1) * degrees / beamList.Count) * (Math.PI / (degrees / 2));
            x = (float)(origin.x + curRadius * Math.Cos(d));
            z = (float)(origin.z + curRadius * Math.Sin(d));

            beamList[i].realPos = new Vector3(x, 0, z);
        }

        var f = (curTick - ticksForSetup) / (float)ticksForSpiral;
        curDegree += Mathf.Lerp(0, 2, f) * TickMultiplier;
        curRadius = Mathf.Lerp(radius, 0, curDegree / degrees);
    }
}