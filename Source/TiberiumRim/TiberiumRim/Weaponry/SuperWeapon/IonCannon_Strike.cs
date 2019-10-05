using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
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
            ActionComposition composition = new ActionComposition();
            composition.AddPart(SoundDef.Named("IonCannon_StartUp"), SoundInfo.OnCamera(), 0f, 3f);
            composition.AddPart(SoundDef.Named("IonCannon_InitialCharge"), SoundInfo.InMap(posInfo), 5f, 5f);
            composition.AddPart(SoundDef.Named("IonCannon_PreClimaxPause"), SoundInfo.InMap(posInfo), 10f);
            composition.AddPart(SoundDef.Named("IonCannon_ClimaxChargeUp"), SoundInfo.InMap(posInfo), 15f);// 10.25f);
            composition.AddPart(SoundDef.Named("IonCannon_Climax"), SoundInfo.InMap(posInfo), 20f);
            composition.AddPart(delegate
            {
                this.Destroy();
            }, 13f);
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

                ActionComposition composition = new ActionComposition();
                composition.AddPart(delegate { DoSetup(composition.curTick); }, SoundDef.Named("IonCannon_StartUp"), SoundInfo.OnCamera(), 0f, 3f);
                composition.AddPart(delegate { DoSpiral(composition.curTick); }, SoundDef.Named("IonCannon_InitialCharge"), SoundInfo.InMap(posInfo), 3f, 5f);
                composition.AddPart(delegate
                {
                    Mote mote = (Mote) ThingMaker.MakeThing(ThingDef.Named("IonBubble"), null);
                    mote.exactPosition = DrawPos;
                    mote.Scale = 20;
                    mote.rotationRate = 1.2f;
                    mote.instanceColor = new ColorInt(70, 90, 175).ToColor;
                    GenSpawn.Spawn(mote, Position, Map, WipeMode.Vanish);
                }, 6.75f);
                composition.AddPart(SoundDef.Named("IonCannon_PreClimaxPause"), SoundInfo.InMap(posInfo), 8f);
                composition.AddPart(SoundDef.Named("IonCannon_ClimaxChargeUp"), SoundInfo.InMap(posInfo), 10f);// 10.25f);
                composition.AddPart(delegate
                {
                    //TODO: Notify Satellite
                }, 12.10f);
                composition.AddPart(delegate
                {
                    IonBeam beam = (IonBeam) ThingMaker.MakeThing(ThingDef.Named("IonBeam"));
                    beam.realPos = DrawPos;
                    beam.durationTicks = 100;
                    beam.width = 4;
                    GenSpawn.Spawn(beam, Position, this.Map);
                    //GenExplosion.DoExplosion(Position, Map, radius, DamageDefOf.Bomb, this, TRUtils.Range(1000, 9999));
                }, SoundDef.Named("IonCannon_Climax"), SoundInfo.InMap(posInfo), 12.10f);
                composition.AddPart(delegate
                {
                    this.Destroy();
                }, 13f);
                composition.Init();
            }
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
