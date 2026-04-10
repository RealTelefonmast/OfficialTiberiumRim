using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;

namespace TiberiumRimFactions
{
    public class IonCannonEffector : ThingWithComps
    {
        //General
        private List<IonBeam> BeamList = new List<IonBeam>();

        private float radius = 25f;

        private int beamAmount = 8;

        private int beamAmountCounter = 0;

        private bool Finished = false;

        // Stage One -- Setup

        public int setupTimeTicks;

        public int setupTimerCounter;

        public int ticksPerPos;

        // Stage Two -- Spiral

        private float radiusDegradeFlt;

        private int spiralTimeTicks;

        private int spiralTimeCounter;

        private float ticksPerDegree;

        private int degrees = 0;

        private bool DoTurn = false;

        private bool FinishedTurn = false;

        // Stage Three -- Climax

        private IonBeam LastBeam = new IonBeam();

        private int climaxTimer;

        private int climaxCounter;

        private bool MoteSpawned = false;

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
            if (Finished)
            {
                this.DeSpawn();
            }

            if (base.Spawned)
            {
                if (DoTurn)

                {
                    if (!FinishedTurn)
                    {
                        if (spiralTimeCounter < spiralTimeTicks)
                        {
                            IntVec3 origin = this.Position;

                            if (this.spiralTimeCounter % (int)ticksPerDegree == 0)
                            {
                                radius = radius - radiusDegradeFlt;

                                for (int i = 0; i < BeamList.Count; i++)
                                {
                                    float x;
                                    float z;
                                    Double d = (degrees + (i - 1) * 360 / BeamList.Count) * (Math.PI / 180);

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
                            foreach (IonBeam beam in BeamList)
                            {
                                beam.DeSpawn();
                            }
                            FinishedTurn = true;
                        }
                    }
                    else
                    {
                        IntVec3 center = this.Position;

                        if (!MoteSpawned)
                        {
                            Mote mote = (Mote)ThingMaker.MakeThing(ThingDefOf.Mote_PowerBeam, null);
                            mote.exactPosition = center.ToVector3Shifted();
                            mote.Scale = 90f;
                            mote.rotationRate = 1.2f;
                            GenSpawn.Spawn(mote, center, this.Map);
                            this.MoteSpawned = !MoteSpawned;
                        }

                        if (climaxCounter < climaxTimer)
                        {
                            if (this.climaxCounter % GenTicks.TicksPerRealSecond * 2 == 0)
                            {
                                if (center.InBounds(this.Map))
                                {
                                    IonBeam thing = (IonBeam)ThingMaker.MakeThing(ThingDef.Named("IonBeam_Final"));
                                    thing.realPosition = new Vector3(center.x, center.y, center.z);
                                    thing.duration = GenTicks.TicksPerRealSecond;
                                    LastBeam = (IonBeam)GenSpawn.Spawn(thing, center, this.Map);
                                }
                                climaxCounter++;
                            }
                            else
                            {
                                DoClimax(center);
                                this.Finished = !Finished;
                            }
                        }
                    }
                }
                else
                {
                    if (setupTimerCounter < setupTimeTicks)
                    {
                        IntVec3 origin = this.Position;
                        if (setupTimerCounter % ticksPerPos == 0)
                        {
                            if (beamAmountCounter < beamAmount)
                            {
                                //Get The Position For Each Beam 
                                float x;
                                float z;
                                Double d = beamAmountCounter * 360 / beamAmount * (Math.PI / 180);

                                x = (float)(origin.x + radius * Math.Cos(d));
                                z = (float)(origin.z + radius * Math.Sin(d));

                                Log.Message("BeamAmountCounter: " + beamAmountCounter + " Bogenmas: " + d);
                                Log.Message("Float X: " + x + " Float Z: " + z);

                                //Spawn The Beam
                                IonBeam IonBeam = new IonBeam();
                                IntVec3 cell = new IntVec3((int)x, 0, (int)z);
                                if (cell.InBounds(this.Map))
                                {
                                    IonBeam thing = (IonBeam)ThingMaker.MakeThing(ThingDef.Named("IonBeam"));
                                    thing.realPosition = new Vector3(x, 0, z);
                                    thing.duration = (this.spiralTimeTicks + this.setupTimeTicks) - setupTimerCounter;
                                    IonBeam = (IonBeam)GenSpawn.Spawn(thing, cell, this.Map);
                                }

                                //Lets shake the camera a bit
                                Find.CameraDriver.shaker.SetMinShake(50);

                                BeamList.Add(IonBeam);
                                beamAmountCounter++;
                            }
                        }
                        setupTimerCounter++;
                    }
                    else
                    {
                        Log.Message("Time To Do Turn");
                        this.DoTurn = true;
                    }
                }
            }
        }
        

        public void DoClimax(IntVec3 center)
        {
            //   GenExplosion.DoExplosion(center, this.Map, this.radius, DamageDefOf.Flame, , 125);
            for (int i = 0; i < 100; i++)
            {
                IntVec3 c = (from x in GenRadial.RadialCellsAround(base.Position, this.radius, true)
                             where x.InBounds(base.Map)
                             select x).RandomElementByWeight((IntVec3 x) => 1f - Mathf.Min(x.DistanceTo(base.Position) / 15f, 1f) + 0.05f);
                FireUtility.TryStartFireIn(c, base.Map, Rand.Range(0.4f, 0.925f));
            }
        }
    }
}


