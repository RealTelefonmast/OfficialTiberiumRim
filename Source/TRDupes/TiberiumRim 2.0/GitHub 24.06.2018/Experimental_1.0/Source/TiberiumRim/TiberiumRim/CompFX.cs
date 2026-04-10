using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;

namespace TiberiumRim
{
    public class CompFX : ThingComp
    {
        private bool effectBool;

        private List<Vector3> MotePositions = new List<Vector3>();

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            MotePositions = SetMotePositions;
        }

        public override void PostExposeData()
        {
            Scribe_Values.Look(ref effectBool, "effectBool");
            base.PostExposeData();
        }

        public CompFX_Properties Props
        {
            get
            {
                return (CompFX_Properties)base.props;
            }
        }

        public override void CompTick()
        {
            base.CompTick();
            if (ShouldDoEffectNow)
            {
                if (Props.Modes.Contains(FxMode.ThrowSmoke))
                {
                    if (this.parent.IsHashIntervalTick(Props.tickRange.RandomInRange))
                    {
                        foreach (Vector3 v in MotePositions)
                        {
                            ThrowMote(v, this.parent.Map);
                        }
                    }
                }
            }
        }

        public bool ShouldDoEffectNow
        {
            get
            {
                return effectBool;
            }
            set
            {
                effectBool = value;
            }
        }

        public List<Vector3> SetMotePositions
        {
            get
            {
                List<Vector3> vList = new List<Vector3>();
                Vector3 v = this.parent.TrueCenter();
                if (this.parent.Graphic.data.graphicClass == typeof(Graphic_Single))
                {
                    for (int i = 0; i < Props.vectorsNorth.Count; i++)
                    {                       
                        Vector3 v2 = v + Props.vectorsNorth[i];
                        Vector3 newVec = new Vector3(v2.x, Altitudes.AltitudeFor(AltitudeLayer.MoteOverhead), v2.z);
                        vList.Add(newVec);
                    }
                }
                else
                {
                    Rot4 rot = parent.Rotation;
                    if (rot == Rot4.South)
                    {
                        for (int i = 0; i < Props.vectorsNorth.Count; i++)
                        {
                            Vector3 v2 = v + Props.vectorsNorth[i];
                            Vector3 newVec = new Vector3(v2.x, Altitudes.AltitudeFor(AltitudeLayer.MoteOverhead), v2.z);
                            vList.Add(newVec);
                        }
                    }
                    if(rot == Rot4.East)
                    {
                        for (int i = 0; i < Props.vectorsWestEast.Count; i++)
                        {
                            Vector3 v2 = Props.vectorsWestEast[i];
                            Vector3 newVec = new Vector3(v.x - v2.x, Altitudes.AltitudeFor(AltitudeLayer.MoteOverhead), v2.z + v.z);
                            vList.Add(newVec);
                        }
                    }
                    if (rot == Rot4.West)
                    {
                        for (int i = 0; i < Props.vectorsWestEast.Count; i++)
                        {
                            Vector3 v2 = Props.vectorsWestEast[i];
                            Vector3 newVec = new Vector3(v2.x + v.x, Altitudes.AltitudeFor(AltitudeLayer.MoteOverhead), v2.z + v.z);
                            vList.Add(newVec);
                        }
                    }
                    if (rot == Rot4.North)
                    {
                        for (int i = 0; i < Props.vectorsSouth.Count; i++)
                        {
                            Vector3 v2 = v + Props.vectorsSouth[i];
                            Vector3 newVec = new Vector3(v2.x, Altitudes.AltitudeFor(AltitudeLayer.MoteOverhead), v2.z);
                            vList.Add(newVec);
                        }
                    }
                }
                return vList;
            }
        }

        public void ThrowMote(Vector3 loc, Map map)
        {
            if (!loc.ShouldSpawnMotesAt(map) || map.moteCounter.SaturatedLowPriority)
            {
                return;
            }
            MoteThrown moteThrown = (MoteThrown)ThingMaker.MakeThing(Props.moteDef, null);
            moteThrown.Scale = Rand.Range(1.5f, 2.5f) * Props.moteSize;
            moteThrown.rotationRate = Rand.Range(-30f, 30f);
            moteThrown.exactPosition = loc;
            float wspd = this.parent.Map.windManager.WindSpeed;
            float spd = wspd > 0.5f ? (wspd > 1f ? 1f : wspd) : 0.5f;
            moteThrown.SetVelocity((float)Rand.Range(30, 40), spd);
            GenSpawn.Spawn(moteThrown, loc.ToIntVec3(), map);
        }
    }

    public class CompFX_Properties : CompProperties
    {
        public List<FxMode> Modes;

        public List<Vector3> vectorsNorth;
        public List<Vector3> vectorsWestEast;
        public List<Vector3> vectorsSouth;

        public IntRange tickRange;
        public ThingDef moteDef;
        public float moteSize = 0.5f;

        public int duration;

        public CompFX_Properties()
        {
            this.compClass = typeof(CompFX);
        }
    }

    public enum FxMode
    {
        CauseGlow,
        ThrowSmoke,
        ThrowSpray
    }
}
