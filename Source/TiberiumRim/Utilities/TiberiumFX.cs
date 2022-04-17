using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public static class TiberiumFX
    {
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
            Mote_Arc arc = (Mote_Arc)ThingMaker.MakeThing(TiberiumDefOf.Mote_Arc);
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

        public static void DoAscensionParticlesInRadius(IntVec3 center, Map map, float radius, float duration, IntRange frequency)
        {
            List<IntVec3> cachedList = GenRadial.RadialCellsAround(center, radius, true).InRandomOrder().ToList();
            int i = 0;
            ActionComposition composition = new ActionComposition("Ascension Particles");
            composition.AddPart(delegate (ActionPart part)
            {
                if (part.CurrentTick % TRandom.Range(frequency) == 0)
                {
                    AscensionParticle(cachedList[i], map);
                }

                i++;
                if (i == cachedList.Count)
                {
                    i = 0;
                }
            }, 0, duration);
            composition.Init();
        }

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
                        TRMote particle = (TRMote) ThingMaker.MakeThing(ThingDef.Named("IonAscensionCloud"), null);
                        particle.exactPosition = exactPos;
                        particle.Scale = TRandom.Range(0.5f, 2.5f);
                        particle.exactRotation = TRandom.Range(0, 360);
                        particle.instanceColor = new ColorInt(70, 90, 175).ToColor;
                        particle.rotationRate = 1.75f;
                        particle.Speed = TRandom.Range(0.5f, 1.5f);
                        if (useFallOff)
                        {
                            var pct = Mathf.InverseLerp(0, radius, center.DistanceTo(cachedList[i]));
                            particle.solidTimeOverride = Mathf.Lerp(0, particle.def.mote.solidTime, 1-pct);
                            particle.fadeOutTimeOverride = Mathf.Lerp(0, particle.def.mote.solidTime, 1-pct);
                        }

                        GenSpawn.Spawn(particle, cachedList[i], map);
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

        public static void AscensionParticle(IntVec3 pos, Map map)
        {
            Mote mote = (Mote) ThingMaker.MakeThing(ThingDef.Named("IonBeamBurn"), null);
            TRMote mote2 = (TRMote) ThingMaker.MakeThing(ThingDef.Named("IonParticle"), null);
            mote.exactPosition = mote2.exactPosition = pos.ToVector3Shifted();
            mote.Scale = 3 * TRandom.Range(1.5f, 3f);
            mote2.Scale = 1 * TRandom.Range(0.5f, 1f);
            mote.rotationRate = 1.2f;
            mote2.rotationRate = 1.2f;
            mote.instanceColor = new ColorInt(70, 90, 175).ToColor;
            GenSpawn.Spawn(mote, pos, map);
            GenSpawn.Spawn(mote2, pos, map);
        }

        public static void ThrowTiberiumGlow(IntVec3 c, Map map, float size)
        {
            Vector3 vector = c.ToVector3Shifted();
            vector += size * new Vector3(Rand.Value - 0.5f, 0f, Rand.Value - 0.5f);
            if (!vector.InBounds(map))
            {
                return;
            }
            MoteThrown moteThrown = (MoteThrown)ThingMaker.MakeThing(ThingDef.Named("Mote_TiberiumGlow"), null);
            moteThrown.Scale = Rand.Range(4f, 6f) * size;
            moteThrown.rotationRate = Rand.Range(-3f, 3f);
            moteThrown.exactPosition = vector;
            moteThrown.SetVelocity((float)Rand.Range(0, 360), 0.12f);
            GenSpawn.Spawn(moteThrown, vector.ToIntVec3(), map, WipeMode.Vanish);
        }
    }
}
