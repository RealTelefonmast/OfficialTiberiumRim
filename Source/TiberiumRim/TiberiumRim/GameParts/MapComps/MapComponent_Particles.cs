using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;

namespace TiberiumRim
{
    public class MapComponent_Particles : MapComponent
    {
        public HashSet<Particle> SavedParticles = new HashSet<Particle>();
        //public List<List<Particle>> ParticleGroups = new List<List<Particle>>();
        //public IEnumerator GroupEnumerator;

        public MapComponent_Particles(Map map) : base(map)
        {}

        public override void FinalizeInit()
        {
            base.FinalizeInit();
            foreach(Particle particle in SavedParticles)
            {
                ParticleMaker.SpawnParticle(map, particle.Position, particle, true);
            }
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
            for (int i = particles.Count() - 1; i >= 0; i--)
            {
                particles[i].Tick();
            }
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
}
