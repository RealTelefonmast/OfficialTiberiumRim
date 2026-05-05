using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace TeleCore.Rendering.Particles;

public class MapComponent_Particles : MapComponent
{
    public HashSet<Particle> SavedParticles = new();
    //public List<List<Particle>> ParticleGroups = new List<List<Particle>>();
    //public IEnumerator GroupEnumerator;

    public MapComponent_Particles(Verse.Map map) : base(map)
    {
    }

    public override void FinalizeInit()
    {
        base.FinalizeInit();
        foreach (var particle in SavedParticles) ParticleMaker.SpawnParticle(map, particle.Position, particle, true);
    }

    public override void MapComponentUpdate()
    {
        base.MapComponentUpdate();
    }

    public IEnumerator Ticker()
    {
        yield return null;
        ParticleTicks();
    }

    public void ParticleTicks()
    {
        var particles = SavedParticles.ToArray();
        for (var i = particles.Count() - 1; i >= 0; i--) particles[i].Tick();
    }

    public override void ExposeData()
    {
        Scribe_Collections.Look(ref SavedParticles, "currentParticles", LookMode.Deep);
        base.ExposeData();
    }

    public void RegisterParticle(Particle particle)
    {
        SavedParticles.Add(particle);
    }

    public void DeregisterParticle(Particle particle)
    {
        SavedParticles.Remove(particle);
    }
}

public class ParticleList
{
    private readonly List<List<Particle>> particleLists = new();
    private readonly List<Particle> particlesToDeregister = new();
    private readonly List<Particle> particlesToRegister = new();

    public ParticleList()
    {
        particleLists.Add(new List<Particle>());
    }

    public void Tick()
    {
        for (var i = 0; i < particlesToRegister.Count; i++) particleLists[0].Add(particlesToRegister[i]);
        particlesToRegister.Clear();
        for (var i = 0; i < particlesToDeregister.Count; i++) particleLists[0].Remove(particlesToDeregister[i]);
        particlesToDeregister.Clear();

        var list2 = particleLists[0];
        for (var m = 0; m < list2.Count; m++)
            if (list2[m].Destroyed)
                continue;
    }

    public void RegisterThing(Particle p)
    {
        particlesToRegister.Add(p);
    }

    public void DeregisterThing(Particle p)
    {
        particlesToDeregister.Add(p);
    }

    public void Reset()
    {
        for (var i = 0; i < particleLists.Count; i++) particleLists[i].Clear();
        particlesToRegister.Clear();
        particlesToDeregister.Clear();
    }

    public void RemoveWhere(Predicate<Particle> predicate)
    {
        for (var i = 0; i < particleLists.Count; i++) particleLists[i].RemoveAll(predicate);
        particlesToRegister.RemoveAll(predicate);
        particlesToDeregister.RemoveAll(predicate);
    }

    private List<Particle> BucketOf(Particle p)
    {
        return particleLists[0];
    }
}