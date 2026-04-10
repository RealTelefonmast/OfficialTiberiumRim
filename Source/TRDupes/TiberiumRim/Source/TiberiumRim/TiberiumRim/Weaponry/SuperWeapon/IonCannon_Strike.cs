using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using Verse;
using UnityEngine;
using Verse.Sound;

namespace TiberiumRim
{
    public class IonCannon_Strike : ThingWithComps
    {
        //
        public AttackSatellite_Ion satellite;
        private List<IonBeam> beamList = new List<IonBeam>();
        private List<IntVec3> beamPositions = new List<IntVec3>();
        private static readonly float radius = 25f;
        private static readonly float degrees = 360;
        private static readonly int beamCount = 8;
        private float totalSeconds = 12;

        private float curRadius = 0;
        private int curBeamCount = 0;

        //Stage One -- Setup
        public int ticksForSetup;

        public int ticksPerBeam;

        //Stage Two -- Spiral
        public int ticksForSpiral;
        public float radiusPerDegree;
        public int ticksPerDegree;

        public float curDegree = 0;

        //Stage Three -- Climax
        public override void ExposeData()
        {
            base.ExposeData();
        }

        //TODO: Test Vanilla Sound System
        private void IonCannonSoundTest()
        {
            var posInfo = new TargetInfo(Position, Map);
            ActionComposition composition = new ActionComposition("IonCannon Strike SoundTest");
            composition.AddPart(SoundDef.Named("IonCannon_StartUp"), SoundInfo.OnCamera(), 0f, 3f);
            composition.AddPart(SoundDef.Named("IonCannon_InitialCharge"), SoundInfo.InMap(posInfo), 5f, 5f);
            composition.AddPart(SoundDef.Named("IonCannon_PreClimaxPause"), SoundInfo.InMap(posInfo), 10f);
            composition.AddPart(SoundDef.Named("IonCannon_ClimaxChargeUp"), SoundInfo.InMap(posInfo), 15f);// 10.25f);
            composition.AddPart(SoundDef.Named("IonCannon_Climax"), SoundInfo.InMap(posInfo), 20f);
            composition.AddFinishAction(delegate
            {
                this.Destroy();
            });
            composition.Init();
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
                ticksPerDegree = ticksForSpiral / (int) degrees;

                //
                SetupPositions();
                Log.Message("## Setting Up Ion Cannon ##");
                Log.Message("Radius: " + radius + " RadiusDownGrade: " + radiusPerDegree);
                Log.Message("Spiral Ticks: " + ticksForSpiral + " ticks per degree: " + ticksPerDegree);
                Log.Message("Setup Ticks: " + ticksForSetup + " ticks per beam: " + ticksPerBeam);

                var posInfo = new TargetInfo(Position, map);

                ActionComposition composition = new ActionComposition("Ion Cannon Strike");
                composition.AddPart(delegate { StartDustEffecter(Position, radius, 12); }, 0);
                composition.AddPart(delegate { DoSetup(composition.CurrentTick); }, SoundDef.Named("IonCannon_StartUp"),
                    SoundInfo.InMap(posInfo, MaintenanceType.PerFrame), 0f, 3f);
                composition.AddPart(delegate { DoSpiral(composition.CurrentTick); },
                    SoundDef.Named("IonCannon_InitialCharge"), SoundInfo.InMap(posInfo, MaintenanceType.PerFrame), 3f,
                    5f);
                composition.AddPart(delegate
                {
                    MakeIonBubble(5, radius, new ColorInt(70, 90, 175).ToColor, ThingDef.Named("IonDistortionBubble"));
                    MakeIonBubble(5, radius * 0.4f, new ColorInt(100, 120, 175).ToColor, ThingDef.Named("IonCenterDistortionBubble"));

                }, 7f); // 5 second alive time for bubble
                composition.AddPart(SoundDef.Named("IonCannon_PreClimaxPause"),
                    SoundInfo.InMap(posInfo, MaintenanceType.PerFrame), 8f);
                composition.AddPart(SoundDef.Named("IonCannon_ClimaxChargeUp"),
                    SoundInfo.InMap(posInfo, MaintenanceType.PerFrame), 10f); // 10.25f);
                composition.AddPart(delegate
                {
                    //TODO: Notify Satellite; Draw laser on world uwu

                }, 11f);
                composition.AddPart(delegate
                {
                    IonBeam beam = (IonBeam) ThingMaker.MakeThing(ThingDef.Named("IonBeam"));
                    beam.realPos = DrawPos;
                    beam.durationTicks = 60;
                    beam.width = 8;
                    beam.continuousBurn = false;
                    GenSpawn.Spawn(beam, Position, this.Map);
                    //GenExplosion.DoExplosion(Position, Map, radius, DamageDefOf.Bomb, this, TRUtils.Range(1000, 9999));
                    foreach (var intVec3 in GenRadial.RadialCellsAround(Position, radius, true))
                    {
                        var list = intVec3.GetThingList(Map);
                        for (var i = 0; i < list.Count; i++)
                        {
                            var thing = list[i];

                            var dinfo = new DamageInfo(DamageDefOf.Burn,
                                Mathf.Lerp(3000, 100, Position.DistanceTo(intVec3) / radius), 4);
                            thing?.TakeDamage(dinfo);
                        }
                    }
                    Mote mote = (Mote) ThingMaker.MakeThing(ThingDef.Named("IonBurnMark"));
                    mote.exactPosition = DrawPos;
                    mote.Scale = radius * 6.5f;
                    mote.rotationRate = 1.2f;
                    GenSpawn.Spawn(mote, Position, Map);
                    //MakeIonBubble(2, 8, new ColorInt(120, 140, 225).ToColor, ThingDef.Named("IonCenterDistortionBubble"));
                    //MoteMaker.MakeStaticMote(Position, Map, ThingDef.Named("IonExplosionShockwave"), radius);

                    ActionComposition ionExpComp = new ActionComposition("Ion Last Exp ");
                    Mote distortion = (Mote)ThingMaker.MakeThing(ThingDef.Named("IonExplosionShockwave"));
                    ionExpComp.AddPart(delegate
                    {
                        distortion.exactPosition = DrawPos;
                        distortion.rotationRate = 1.2f;
                        GenSpawn.Spawn(distortion, Position, Map);
                    }, 0);
                    ionExpComp.AddPart(delegate (ActionPart part)
                    {
                        distortion.Scale = radius * (part.CurrentTick / (float)part.playTime);
                    }, 0, 0.75f);
                    ionExpComp.Init();



                }, SoundDef.Named("IonCannon_Climax"), SoundInfo.InMap(posInfo, MaintenanceType.PerFrame), 12f);
                composition.AddFinishAction(delegate
                {
                    Log.Message("Ion Cannon Should Be Destroyed Now");
                    this.Destroy();
                });
                composition.Init();
            }
        }

        //TODO: Make new Mote class, control fade-in and fade-out times directly, instead over mote properties 
        //TODO: Make new mote overridable, no more static mote props.
        //
        //

        private void MakeIonBubble(float time, float bubbleRadius, Color color, ThingDef ionDistortion)
        {
            ActionComposition composition = new ActionComposition("Ion Bübble " + ionDistortion);
            Mote mote = (Mote)ThingMaker.MakeThing(ThingDef.Named("IonBubble"));
            Mote distortion = (Mote)ThingMaker.MakeThing(ionDistortion);
            composition.AddPart(delegate
            {
                mote.exactPosition = distortion.exactPosition = DrawPos;
                mote.Scale = bubbleRadius;
                mote.rotationRate = distortion.rotationRate = 1.2f;
                mote.instanceColor = color;
                GenSpawn.Spawn(mote, Position, Map);
                GenSpawn.Spawn(distortion, Position, Map);
            }, 0);
            composition.AddPart(delegate(ActionPart part)
            {
                distortion.Scale = mote.Scale = bubbleRadius * (part.CurrentTick / (float)part.playTime);
            }, 0, time);
            composition.Init();
        }

        private void StartDustEffecter(IntVec3 around, float radius, float duration)
        {
            IEnumerator<IntVec3> cachedCells = null;
            Color color = new ColorInt(15, 15, 55).ToColor;
            ActionComposition composition = new ActionComposition("Dust Effecter");
            composition.AddPart(delegate
            {
                Log.Message("## Starting dust puffing");
                cachedCells = GenRadial.RadialCellsAround(around, radius, true).InRandomOrder().GetEnumerator();
            },0);
            composition.AddPart(delegate (ActionPart part)
            {
                if (part.CurrentTick % 4 == 0)
                {
                    MoteThrown moteThrown = (MoteThrown)ThingMaker.MakeThing(ThingDefOf.Mote_DustPuffThick, null);
                    moteThrown.Scale = 1.9f * TRUtils.Range(2f, 5f);
                    moteThrown.rotationRate = (float)Rand.Range(-60, 60);
                    moteThrown.exactPosition = cachedCells.Current.ToVector3Shifted();
                    moteThrown.instanceColor = color;
                    moteThrown.SetVelocity((float)Rand.Range(0, 360), TRUtils.Range(0.6f, 0.75f));
                    GenSpawn.Spawn(moteThrown, cachedCells.Current, Map);
                    if (!cachedCells.MoveNext())
                    {
                        cachedCells.Reset();
                    }
                }

            }, 0, duration);
            composition.AddFinishAction(delegate
            {
                Log.Message("## Ending dust puffing...");
                cachedCells.Dispose();
            });
            composition.Init();
        }

        private void SetupPositions()
        {
            Vector3 origin = DrawPos;
            float x;
            float z;
            while (beamPositions.Count < 8)
            {
                double d = beamPositions.Count * degrees / beamCount * (Math.PI / 180);
                x = (float) (origin.x + curRadius * Math.Cos(d));
                z = (float) (origin.z + curRadius * Math.Sin(d));
                beamPositions.Add(new IntVec3((int)x, 0, (int)z));
            }
        }

        private void DoSetup(int tick)
        {
            Vector3 origin = DrawPos;//Position.ToVector3Shifted();
            if (tick % ticksPerBeam == 0 && curBeamCount < beamCount)
            {
                IntVec3 pos = beamPositions.RandomElement();
                beamPositions.Remove(pos);
                if (pos.InBounds(Map))
                {
                    IonBeam beam = (IonBeam)ThingMaker.MakeThing(ThingDef.Named("IonBeam"));
                    beam.realPos = pos.ToVector3Shifted();
                    beam.durationTicks = TotalTime - tick + GenTicks.SecondsToTicks(1);
                    GenSpawn.Spawn(beam, pos, this.Map);
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
            Vector3 origin = Position.ToVector3Shifted();
            for(int i = 0; i < beamList.Count; i++)
            {
                float x;
                float z;
                Double d = (curDegree + (i - 1) * degrees / beamList.Count) * (Math.PI / (degrees / 2));
                x = (float)(origin.x + curRadius * Math.Cos(d));
                z = (float)(origin.z + curRadius * Math.Sin(d));

                beamList[i].realPos = new Vector3(x, 0, z);
            }
            var f = (curTick - ticksForSetup) / (float) ticksForSpiral;
            curDegree += Mathf.Lerp(0, 2, f) * TickMultiplier;
            curRadius = Mathf.Lerp(radius, 0, curDegree/ degrees);
        }

        public int TotalTime => ticksForSetup + ticksForSpiral;
        public float TickMultiplier => (degrees / ticksForSpiral);
    }
}
