using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.Noise;
using RimWorld;
using UnityEngine;
using System.Collections;

namespace TiberiumRim
{
    public class Particle : Entity, IExposable
    {
        public ParticleDef def;
        public Map map;
        public Vector3 exactPos;
        public float exactScale = 1f;
        public float exactRotation = 0f;
        public IntVec3 startCell;
        public IntVec3 endCell = IntVec3.Invalid;

        private float angle;
        private float speed = 1f;
        private float wiggleMin;
        private float wiggleMax;
        private float sizeMin;
        private float sizeMax;
        private float lifeDistance;
        //Ticking And Life
        //private static ref int ticksGame = FieldRefAccess<TickManager, int>(Find.TickManager, "ticksGameInt");
        private sbyte mapIndexOrState = -1;

        private int tickOffset = 0;
        private int lifeTicks = 0;
        private int ticksRemaining = 0;
        private int solidTicks;
        private int fadoutTicks;
        private int fadeinTicks;


        private Graphic_Particle graphic;
        private Color color = Color.white;
        private Color colorTwo = Color.white;
        private Vector3 velocity;

        public Particle(){}

        public virtual void PreSpawnSetup(IntVec3 spawnCell, Map map)
        {
            this.map = map;
            exactPos = spawnCell.ToVector3Shifted();
            exactPos.y = def.altitudeLayer.AltitudeFor();
            startCell = spawnCell;
        }

        public override void SpawnSetup(Map map, bool respawning)
        {
            angle = startCell.ToVector3().AngleToFlat(endCell.ToVector3());           
            wiggleMin = def.wiggleRange.min;
            wiggleMax = def.wiggleRange.max;
            sizeMin = def.sizeRange.min;
            sizeMax = def.sizeRange.max;
            exactScale = sizeMin;
            exactRotation = def.rotationSpeed;
            solidTicks = GenTicks.SecondsToTicks(def.solidTime);
            fadoutTicks = GenTicks.SecondsToTicks(def.fadeOutTime);
            fadeinTicks = GenTicks.SecondsToTicks(def.fadeInTime);
            lifeTicks = fadeinTicks + solidTicks + fadoutTicks;
            Vector3 vel = new Vector3(def.direction.x, 0, def.direction.y);
            mapIndexOrState = (sbyte)Find.Maps.IndexOf(map);
            if (endCell.IsValid)
            {
                int x = endCell.x - startCell.x;
                int z = endCell.z - startCell.z;
                vel = new Vector3(x, 0, z);
                lifeDistance = startCell.DistanceTo(endCell);
            }
            velocity = vel.normalized;
            if (!respawning)
            {
                map.GetComponent<MapComponent_Particles>().RegisterParticle(this);
                tickOffset = TRUtils.Range(0, 999);
                speed = TRUtils.Range(def.speedRange);
                ticksRemaining = lifeTicks;
            }
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            map.GetComponent<MapComponent_Particles>().DeregisterParticle(this);
            mapIndexOrState = -3;
            Log.Message("Should be destroyed now.");
        }

        public virtual void ExposeData()
        {
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (!def.shouldBeSaved)
                {
                    map.GetComponent<MapComponent_Particles>().DeregisterParticle(this);
                }
            }
            Scribe_Defs.Look(ref def, "def");
            Scribe_References.Look(ref map, "map");
            Scribe_Values.Look(ref exactPos, "exactPos");
            Scribe_Values.Look(ref exactScale, "exactScale");
            Scribe_Values.Look(ref startCell, "startCell");
            Scribe_Values.Look(ref endCell, "endCell");
            Scribe_Values.Look(ref tickOffset, "tickOffset");
            Scribe_Values.Look(ref speed, "speed");
            Scribe_Values.Look(ref ticksRemaining, "remaining");
        }

        public override void Tick()
        {
            if (Destroyed) return;
            if (!Position.InBounds(map))
                DeSpawn();
            if (ShouldDestroy)
            {
                FinishAction();
                DeSpawn();
                return;
            }
            int tick = Find.TickManager.TicksGame + tickOffset;
            float sizeRange = sizeMin;
            if (sizeMin != sizeMax)
            {
               sizeRange = TRUtils.Cosine(sizeMin, sizeMax, def.frequency, tick);
            }
            float directionOffset = wiggleMin;
            if (wiggleMin != wiggleMax)
            {
                directionOffset = TRUtils.Cosine(wiggleMin, wiggleMax, def.frequency, tick);
            }
            exactScale = sizeRange;
            exactPos = this.exactPos + (velocity * speed) * 0.0166666675f;
            if (wiggleMin != wiggleMax)
            {
                float val = TRUtils.Cosine2(-1f, 1f, 90f, 0f, angle);
                exactPos.x += Mathf.Lerp(directionOffset, 0f, Mathf.Abs(val));
                exactPos.z += Mathf.Lerp(0f, directionOffset, Mathf.Abs(val));
            }
            ticksRemaining--;
        }

        public override string Label => "";
        public override string LabelCap => "";
        public virtual void FinishAction()
        {
            mapIndexOrState = -3;
        }

        public virtual bool ShouldDestroy
        {
            get
            {
                if (ticksRemaining <= 0)
                    return true;
                if (startCell.DistanceTo(Position) >= lifeDistance)
                    return true;
                if (endCell.IsValid && Position == endCell)
                    return true;
                return false;
            }
        }

        public bool Destroyed
        {
            get
            {
                return (int)this.mapIndexOrState == -2 || (int)this.mapIndexOrState == -3;
            }
        }

        public float AgeSecs
        {
            get
            {
                return (float)(lifeTicks - ticksRemaining) / 60f;
            }
        }

        public float Alpha
        {
            get
            {
                int age = lifeTicks - ticksRemaining;
                if (age < fadeinTicks)
                {
                    return (float)age / (float)fadeinTicks;
                }
                else
                if (age < fadeinTicks + solidTicks)
                {
                    return 1f;
                }
                else
                if(age < lifeTicks)
                {
                    return 1f- Mathf.InverseLerp(fadoutTicks + solidTicks, lifeTicks, age);
                }
                return 0f;
            }
        }

        public IntVec3 Position
        {
            get
            {
                return exactPos.ToIntVec3();
            }
        }

        public virtual Color Color
        {
            get
            {
                color = def.graphicData.color;
                color.a = Alpha;
                return color;
            }
            set
            {
                color = value;
            }
        }

        public virtual Color ColorTwo
        {
            get
            {
                colorTwo = def.graphicData.colorTwo;
                colorTwo.a = Alpha;
                return colorTwo;
            }
            set
            {
                colorTwo = value;
            }
        }

        public virtual Graphic_Particle Graphic
        {
            get
            {
                if (this.graphic == null)
                {
                    if (this.def.graphicData == null)
                    {
                        return BaseContent.BadGraphic as Graphic_Particle;
                    }
                    this.graphic = this.def.graphicData.GraphicColoredFor(this, color, colorTwo) as Graphic_Particle;
                }
                return this.graphic;
            }
        }

        public virtual void Draw()
        {
            Graphic.DrawParticle(exactPos, this, (int)def.altitudeLayer.AltitudeFor());
        }
    }
}
