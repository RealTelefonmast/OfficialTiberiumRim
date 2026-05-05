using System;
using TeleCore.Defs;
using TeleCore.Particles;
using TeleCore.Types.Entities;
using UnityEngine;
using Verse;

namespace TeleCore.Types.Utils;

public static class ParticleMaker
{
    public static void ThrowParticle(Map map, ParticleDef def, Vector3 pos, Vector3 velocity, float scale = 1f)
    {
        if (def.particleSystemClass != null)
        {
            var manager = map.GetComponent<MapComponent_ParticleManager>();
            var system = manager.GetSystem(def.particleSystemClass);
            if (system is SimpleParticleSystem simpleSystem)
            {
                simpleSystem.Spawn(new BaseParticleData
                {
                    def = def,
                    pos = pos,
                    velocity = velocity,
                    scale = scale,
                    rotation = Rand.Range(0, 360)
                });
            }
            return;
        }
        
        // Fallback to old system if no system class defined
        SpawnParticle(map, pos.ToIntVec3(), def);
    }

    public static Particle SpawnParticle(Map map, IntVec3 cell, ParticleDef particle)
    {
        var particle2 = MakeParticle(particle);
        return SpawnParticle(map, cell, particle2);
    }

    public static Particle SpawnParticle(Map map, IntVec3 cell, Particle particle, bool respawning = false)
    {
        particle.PreSpawnSetup(cell, map);
        particle.SpawnSetup(map, respawning);
        return particle;
    }

    public static Particle SpawnParticleWithPath(IntVec3 start, IntVec3 end, Map map, ParticleDef particle)
    {
        var particle2 = MakeParticle(particle);
        return SpawnParticleWithPath(start, end, map, particle2);
    }

    public static Particle SpawnParticleWithPath(IntVec3 start, IntVec3 end, Map map, Particle particle)
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