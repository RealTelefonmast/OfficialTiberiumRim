using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using TiberiumRim;
using UnityEngine;
using Verse;

namespace TiberiumRimFactions;

public class IonCannonEffector : ThingWithComps
{
    private readonly int beamAmount = 8;

    //General
    private readonly List<IonBeam> BeamList = new();

    private int beamAmountCounter;

    private int climaxCounter;

    private int climaxTimer;

    private int degrees;

    private bool DoTurn;

    private bool Finished;

    private bool FinishedTurn;

    // Stage Three -- Climax

    private IonBeam LastBeam = new IonBeam();

    private bool MoteSpawned;

    private float radius = 25f;

    // Stage Two -- Spiral

    private float radiusDegradeFlt;

    public int setupTimerCounter;

    // Stage One -- Setup

    public int setupTimeTicks;

    private int spiralTimeCounter;

    private int spiralTimeTicks;

    private float ticksPerDegree;

    public int ticksPerPos;

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
            setupTimeTicks = GenTicks.SecondsToTicks(6);
            ticksPerPos = setupTimeTicks / (beamAmount - 1);

            //Spiral
            spiralTimeTicks = GenTicks.SecondsToTicks(6);
            radiusDegradeFlt = radius / 360;
            ticksPerDegree = spiralTimeTicks / 360;

            //
            climaxTimer = GenTicks.SecondsToTicks(3);

            Log.Message("radius: " + radius + " RadiusDownGrade: " + radiusDegradeFlt);
            Log.Message("turnticks: " + spiralTimeTicks + " jumpInt: " + ticksPerDegree);
            Log.Message("setupTimeTicks: " + setupTimeTicks + " ticksPerPos: " + ticksPerPos);
        }
    }

    public override void Tick()
    {
        if (Finished) DeSpawn();

        if (Spawned)
        {
            if (DoTurn)

            {
                if (!FinishedTurn)
                {
                    if (spiralTimeCounter < spiralTimeTicks)
                    {
                        var origin = Position;

                        if (spiralTimeCounter % (int)ticksPerDegree == 0)
                        {
                            radius = radius - radiusDegradeFlt;

                            for (var i = 0; i < BeamList.Count; i++)
                            {
                                float x;
                                float z;
                                var d = (degrees + (i - 1) * 360 / BeamList.Count) * (Math.PI / 180);

                                x = (float)(origin.x + radius * Math.Cos(d));
                                z = (float)(origin.z + radius * Math.Sin(d));

                                BeamList[i].realPosition = new Vector3(x, 0, z);
                            }

                            degrees++;
                        }

                        spiralTimeCounter++;
                    }
                    else
                    {
                        foreach (IonBeam beam in BeamList) beam.DeSpawn();
                        FinishedTurn = true;
                    }
                }
                else
                {
                    var center = Position;

                    if (!MoteSpawned)
                    {
                        var mote = (Mote)ThingMaker.MakeThing(ThingDefOf.Mote_PowerBeam);
                        mote.exactPosition = center.ToVector3Shifted();
                        mote.Scale = 90f;
                        mote.rotationRate = 1.2f;
                        GenSpawn.Spawn(mote, center, Map);
                        MoteSpawned = !MoteSpawned;
                    }

                    if (climaxCounter < climaxTimer)
                    {
                        if (climaxCounter % GenTicks.TicksPerRealSecond * 2 == 0)
                        {
                            if (center.InBounds(Map))
                            {
                                IonBeam thing = (IonBeam)ThingMaker.MakeThing(ThingDef.Named("IonBeam_Final"));
                                thing.realPosition = new Vector3(center.x, center.y, center.z);
                                thing.duration = GenTicks.TicksPerRealSecond;
                                LastBeam = (IonBeam)GenSpawn.Spawn(thing, center, Map);
                            }

                            climaxCounter++;
                        }
                        else
                        {
                            DoClimax(center);
                            Finished = !Finished;
                        }
                    }
                }
            }
            else
            {
                if (setupTimerCounter < setupTimeTicks)
                {
                    var origin = Position;
                    if (setupTimerCounter % ticksPerPos == 0)
                        if (beamAmountCounter < beamAmount)
                        {
                            //Get The Position For Each Beam 
                            float x;
                            float z;
                            var d = beamAmountCounter * 360 / beamAmount * (Math.PI / 180);

                            x = (float)(origin.x + radius * Math.Cos(d));
                            z = (float)(origin.z + radius * Math.Sin(d));

                            Log.Message("BeamAmountCounter: " + beamAmountCounter + " Bogenmas: " + d);
                            Log.Message("Float X: " + x + " Float Z: " + z);

                            //Spawn The Beam
                            IonBeam IonBeam = new IonBeam();
                            var cell = new IntVec3((int)x, 0, (int)z);
                            if (cell.InBounds(Map))
                            {
                                IonBeam thing = (IonBeam)ThingMaker.MakeThing(ThingDef.Named("IonBeam"));
                                thing.realPosition = new Vector3(x, 0, z);
                                thing.duration = spiralTimeTicks + setupTimeTicks - setupTimerCounter;
                                IonBeam = (IonBeam)GenSpawn.Spawn(thing, cell, Map);
                            }

                            //Lets shake the camera a bit
                            Find.CameraDriver.shaker.SetMinShake(50);

                            BeamList.Add(IonBeam);
                            beamAmountCounter++;
                        }

                    setupTimerCounter++;
                }
                else
                {
                    Log.Message("Time To Do Turn");
                    DoTurn = true;
                }
            }
        }
    }


    public void DoClimax(IntVec3 center)
    {
        //   GenExplosion.DoExplosion(center, this.Map, this.radius, DamageDefOf.Flame, , 125);
        for (var i = 0; i < 100; i++)
        {
            var c = (from x in GenRadial.RadialCellsAround(Position, radius, true)
                where x.InBounds(Map)
                select x).RandomElementByWeight(x => 1f - Mathf.Min(x.DistanceTo(Position) / 15f, 1f) + 0.05f);
            FireUtility.TryStartFireIn(c, Map, Rand.Range(0.4f, 0.925f));
        }
    }
}