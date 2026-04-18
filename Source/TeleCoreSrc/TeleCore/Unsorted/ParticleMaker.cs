using System;
using TeleCore.Defs;
using Verse;

namespace TeleCore.Unsorted;

public static class ParticleMaker
{
    public static Particle SpawnParticle(Verse.Map map, IntVec3 cell, ParticleDef particle)
    {
        var particle2 = MakeParticle(particle);
        return SpawnParticle(map, cell, particle2);
    }

    public static Particle SpawnParticle(Verse.Map map, IntVec3 cell, Particle particle, bool respawning = false)
    {
        particle.PreSpawnSetup(cell, map);
        particle.SpawnSetup(map, respawning);
        return particle;
    }

    public static Particle SpawnParticleWithPath(IntVec3 start, IntVec3 end, Verse.Map map, ParticleDef particle)
    {
        var particle2 = MakeParticle(particle);
        return SpawnParticleWithPath(start, end, map, particle2);
    }

    public static Particle SpawnParticleWithPath(IntVec3 start, IntVec3 end, Verse.Map map, Particle particle)
    {
        particle.endCell = end;
        return SpawnParticle(map, start, particle);
    }

    public static Particle MakeParticle(ParticleDef def)
    {
        var particle = (Particle)Activator.CreateInstance(def.particleClass);
        particle.def = def;
        return particle;
    }
}