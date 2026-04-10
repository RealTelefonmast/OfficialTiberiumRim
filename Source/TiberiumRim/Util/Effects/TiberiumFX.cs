using System.Linq;
using RimWorld;
using TeleCore.ActionCompositions;
using TR.TextureContent;
using UnityEngine;
using Verse;

namespace TR.Effects;

public static class TiberiumFX
{
    public static void ZappyZap(IntVec3 pos, Map map, Vector3 from, Vector3 to)
    {
        var composition = new ActionComposition("ZipZap");
        composition.AddPart(delegate { Zap(pos, map, from, to); }, 0);
        composition.AddPart(delegate { Zap(pos, map, from, to); }, 0.25f);
        composition.Init();
    }

    public static void Zap(IntVec3 pos, Map map, Vector3 from, Vector3 to)
    {
        Log.Message("Zapping");
        var arc = (Mote_Arc)ThingMaker.MakeThing(EffectsDefOf.Mote_Arc);
        var mat = MaterialsTesla.Jumps[TRandom.Range(0, 5)];
        arc.fadeInTimeOverride = 0.25f;
        arc.solidTimeOverride = 0.25f;
        arc.fadeOutTimeOverride = 0.85f;
        arc.SetConnections(from, to, mat, Color.white);
        arc.Attach(null);
        GenSpawn.Spawn(arc, pos, map);
    }

    public static void StartDustEffecter(IntVec3 center, Map map, float radius, float duration)
    {
        var cachedList = GenRadial.RadialCellsAround(center, radius, true).InRandomOrder().ToList();
        var i = 0;
        var color = new ColorInt(15, 15, 55).ToColor;
        var composition = new ActionComposition("Dust Effecter");
        composition.AddPart(delegate(ActionPart part)
        {
            if (part.CurrentTick % 4 == 0)
            {
                FleckMaker.ThrowDustPuffThick(cachedList[i].ToVector3Shifted(), map, 1.9f * TRandom.Range(2f, 5f),
                    color);
                i++;
                if (i == cachedList.Count) i = 0;
            }
        }, 0, duration);
        composition.Init();
    }

    public static void ThrowTiberiumGlow(IntVec3 c, Map map, float size)
    {
        var vector = c.ToVector3Shifted();
        if (!vector.ShouldSpawnMotesAt(map)) return;
        vector += size * new Vector3(Rand.Value - 0.5f, 0f, Rand.Value - 0.5f);
        if (!vector.InBounds(map)) return;
        var data = FleckMaker.GetDataStatic(vector, map, EffectsDefOf.Mote_TiberiumGlow, Rand.Range(4f, 6f) * size);
        data.rotationRate = Rand.Range(-3f, 3f);
        data.velocityAngle = Rand.Range(0, 360);
        data.velocitySpeed = 0.12f;
        map.flecks.CreateFleck(data);
    }

    public static void ThrowRadiationGlow(IntVec3 c, Map map, float size)
    {
        var vector = c.ToVector3Shifted();
        if (!vector.ShouldSpawnMotesAt(map)) return;
        vector += size * new Vector3(Rand.Value - 0.5f, 0f, Rand.Value - 0.5f);
        if (!vector.InBounds(map)) return;
        var data = FleckMaker.GetDataStatic(vector, map, EffectsDefOf.RadiationGlow, Rand.Range(4f, 6f) * size);
        data.rotationRate = Rand.Range(-3f, 3f);
        data.velocityAngle = Rand.Range(0, 360);
        data.velocitySpeed = 0.12f;
        map.flecks.CreateFleck(data);
    }

    #region IonCannon

    /// <summary>
    ///     Spawns a glow effect on the ground (terrain) and another particle that ascends and dissipates into the air.
    /// </summary>
    public static void ThrowAscensionParticle(IntVec3 pos, Map map)
    {
        var vector = pos.ToVector3Shifted();
        var groundEffect =
            FleckMaker.GetDataStatic(vector, map, EffectsDefOf.RadiationGlow, TRandom.Range(1.5f, 3f) * 3);
        var ascensionParticle =
            FleckMaker.GetDataStatic(vector, map, EffectsDefOf.RadiationGlow, TRandom.Range(0.5f, 1f));
        groundEffect.rotationRate = 1.2f;
        ascensionParticle.rotationRate = 1.2f;
        groundEffect.instanceColor = new ColorInt(70, 90, 175).ToColor;
        map.flecks.CreateFleck(groundEffect);
        map.flecks.CreateFleck(ascensionParticle);
    }

    public static void DoAscensionParticlesInRadius(IntVec3 center, Map map, float radius, float duration,
        IntRange frequency)
    {
        var cachedList = GenRadial.RadialCellsAround(center, radius, true).InRandomOrder().ToList();
        var i = 0;
        var composition = new ActionComposition("Ascension Particles");
        composition.AddPart(delegate(ActionPart part)
        {
            if (part.CurrentTick % TRandom.Range(frequency) == 0) ThrowAscensionParticle(cachedList[i], map);
            i++;
            if (i == cachedList.Count) i = 0;
        }, 0, duration);
        composition.Init();
    }

    /// <summary>
    ///     Spawns Ion effects in a cylindrical area around a point.
    /// </summary>
    public static void DoFloatingEffectsInRadius(IntVec3 center, Map map, float radius, float duration, bool useFallOff,
        IntRange frequency, IntRange heightRange, IntRange particleCount)
    {
        var cachedList = GenRadial.RadialCellsAround(center, radius, true).InRandomOrder().ToList();
        var i = 0;
        var composition = new ActionComposition("Floating Effects");
        composition.AddPart(delegate(ActionPart part)
        {
            if (part.CurrentTick % TRandom.Range(frequency) == 0)
            {
                var count = TRandom.Range(particleCount);
                for (var ii = 0; ii < count; ii++)
                {
                    var exactPos = cachedList[i].ToVector3Shifted() + new Vector3(0, 0, TRandom.Range(heightRange)) +
                                   Gen.RandomHorizontalVector(0.75f);
                    var particle = FleckMaker.GetDataStatic(exactPos, map, EffectsDefOf.RadiationGlow,
                        TRandom.Range(0.5f, 2.5f));
                    particle.rotation = TRandom.Range(0, 360);
                    particle.instanceColor = new ColorInt(70, 90, 175).ToColor;
                    particle.rotationRate = 1.75f;
                    particle.velocitySpeed = TRandom.Range(0.5f, 1.5f);
                    if (useFallOff)
                    {
                        var pct = Mathf.InverseLerp(0, radius, center.DistanceTo(cachedList[i]));
                        particle.solidTimeOverride = Mathf.Lerp(0, particle.def.solidTime, 1 - pct);
                    }

                    map.flecks.CreateFleck(particle);
                }

                i++;
                if (i == cachedList.Count) i = 0;
            }
        }, 0, duration);
        composition.Init();
    }

    #endregion
}