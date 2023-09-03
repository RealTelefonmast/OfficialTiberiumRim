using System.Collections.Generic;
using System.Linq;
using RimWorld;
using TeleCore;
using UnityEngine;
using Verse;

namespace TR
{
    public static class TiberiumFX
    {
        #region IonCannon

                
        /// <summary>
        /// Spawns an glow effect on the ground (terrain) and another particle that ascends and dissipates into the air
        /// </summary>
        public static void ThrowAscensionParticle(IntVec3 pos, Map map)
        {
            var vector =  pos.ToVector3Shifted();
            var groundEffect = FleckMaker.GetDataStatic(vector, map, EffectsDefOf.RadiationGlow, TRandom.Range(1.5f, 3f) * 3);
            var ascensionParticle = FleckMaker.GetDataStatic(vector, map, EffectsDefOf.RadiationGlow, TRandom.Range(0.5f, 1f) * 1);
            groundEffect.rotationRate = 1.2f;
            ascensionParticle.rotationRate = 1.2f;
            groundEffect.instanceColor = new ColorInt(70, 90, 175).ToColor;
            
            map.flecks.CreateFleck(groundEffect);
            map.flecks.CreateFleck(ascensionParticle);
        }
        
        public static void DoAscensionParticlesInRadius(IntVec3 center, Map map, float radius, float duration, IntRange frequency)
        {
            List<IntVec3> cachedList = GenRadial.RadialCellsAround(center, radius, true).InRandomOrder().ToList();
            int i = 0;
            ActionComposition composition = new ActionComposition("Ascension Particles");
            composition.AddPart(delegate (ActionPart part)
            {
                if (part.CurrentTick % TRandom.Range(frequency) == 0)
                {
                    ThrowAscensionParticle(cachedList[i], map);
                }

                i++;
                if (i == cachedList.Count)
                {
                    i = 0;
                }
            }, 0, duration);
            composition.Init();
        }

        /// <summary>
        /// Spawns Ion effects in a cylindrical area around a point
        /// </summary>
        public static void DoFloatingEffectsInRadius(IntVec3 center, Map map, float radius, float duration, bool useFallOff, IntRange frequency, IntRange heightRange, IntRange particleCount)
        {
            List<IntVec3> cachedList = GenRadial.RadialCellsAround(center, radius, true).InRandomOrder().ToList();
            int i = 0;
            ActionComposition composition = new ActionComposition("Floating Effects");
            composition.AddPart(delegate (ActionPart part)
            {
                if (part.CurrentTick % TRandom.Range(frequency) == 0)
                {
                    int count = TRandom.Range(particleCount);
                    for (int ii = 0; ii < count; ii++)
                    {
                        Vector3 exactPos = cachedList[i].ToVector3Shifted() + new Vector3(0, 0, TRandom.Range(heightRange)) + Gen.RandomHorizontalVector(0.75f);
                        var particle = FleckMaker.GetDataStatic(exactPos, map, EffectsDefOf.RadiationGlow, TRandom.Range(0.5f, 2.5f));
                        //TRMote particle = (TRMote) ThingMaker.MakeThing(EffectsDefOf.IonAscensionCloud, null);
                        particle.rotation = TRandom.Range(0, 360);
                        particle.instanceColor = new ColorInt(70, 90, 175).ToColor;
                        particle.rotationRate = 1.75f;
                        particle.velocitySpeed = TRandom.Range(0.5f, 1.5f);
                        if (useFallOff)
                        {
                            var pct = Mathf.InverseLerp(0, radius, center.DistanceTo(cachedList[i]));
                            particle.solidTimeOverride = Mathf.Lerp(0, particle.def.solidTime, 1-pct);
                            //particle.fadeOutTimeOverride = Mathf.Lerp(0, particle.def.mote.solidTime, 1-pct);
                        }
                        
                        map.flecks.CreateFleck(particle);
                    }

                    i++;
                    if (i == cachedList.Count)
                    {
                        i = 0;
                    }
                }
            }, 0, duration);
            composition.Init();
        }

        #endregion
        
        public static void ZappyZap(IntVec3 pos, Map map, Vector3 from, Vector3 to)
        {
            ActionComposition composition = new ActionComposition("ZipZap");
            composition.AddPart(delegate { Zap(pos, map, from, to); }, 0);
            composition.AddPart(delegate { Zap(pos, map, from, to); }, 0.25f);
            composition.Init();
        }

        public static void Zap(IntVec3 pos, Map map, Vector3 from, Vector3 to)
        {
            Log.Message("Zapping");
            Mote_Arc arc = (Mote_Arc)ThingMaker.MakeThing(EffectsDefOf.Mote_Arc);
            Material mat = MaterialsTesla.Jumps[TRandom.Range(0, 5)];
            arc.fadeInTimeOverride = 0.25f;
            arc.solidTimeOverride = 0.25f;
            arc.fadeOutTimeOverride = 0.85f;
            arc.SetConnections(from, to, mat, Color.white);
            arc.Attach(null);
            GenSpawn.Spawn(arc, pos, map);
        }

        public static void StartDustEffecter(IntVec3 center, Map map, float radius, float duration)
        {
            List<IntVec3> cachedList = GenRadial.RadialCellsAround(center, radius, true).InRandomOrder().ToList();
            int i = 0;
            Color color = new ColorInt(15, 15, 55).ToColor;
            ActionComposition composition = new ActionComposition("Dust Effecter");
            composition.AddPart(delegate (ActionPart part)
            {
                if (part.CurrentTick % 4 == 0)
                {
                    FleckMaker.ThrowDustPuffThick(cachedList[i].ToVector3Shifted(), map, 1.9f * TRandom.Range(2f, 5f), color);

                    /*
                    MoteThrown moteThrown = (MoteThrown)ThingMaker.MakeThing(ThingDefOf.Mote_DustPuffThick, null);
                    moteThrown.Scale = 1.9f * TRandom.Range(2f, 5f);
                    moteThrown.rotationRate = (float)Rand.Range(-60, 60);
                    moteThrown.exactPosition = cachedList[i].ToVector3Shifted();
                    moteThrown.instanceColor = color;
                    moteThrown.SetVelocity((float)Rand.Range(0, 360), TRandom.Range(0.6f, 0.75f));
                    GenSpawn.Spawn(moteThrown, cachedList[i], map);
                    */

                    i++;
                    if (i == cachedList.Count) i = 0;
                }

            }, 0, duration);
            composition.Init();
        }

        public static void ThrowRadiationGlow(IntVec3 c, Map map, float size)
        {
            Vector3 vector = c.ToVector3Shifted();
            if (!vector.ShouldSpawnMotesAt(map, true)) return;
            vector += size * new Vector3(Rand.Value - 0.5f, 0f, Rand.Value - 0.5f);
            if (!vector.InBounds(map)) return;
            
            FleckCreationData dataStatic = FleckMaker.GetDataStatic(vector, map, EffectsDefOf.RadiationGlow, Rand.Range(4f, 6f) * size);
            dataStatic.rotationRate = Rand.Range(-3f, 3f);
            dataStatic.velocityAngle = (float)Rand.Range(0, 360);
            dataStatic.velocitySpeed = 0.12f;
            map.flecks.CreateFleck(dataStatic);
        }
    }
}
