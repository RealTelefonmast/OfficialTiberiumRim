using System.Collections.Generic;
using Verse;

namespace TeleCore.Particles
{
    public abstract class ParticleSystem
    {
        public abstract void Tick();
        public abstract void Draw(); // We can add DrawBatch here if needed
        public abstract void ExposeData();
    }

    public abstract class ParticleSystemBase<T> : ParticleSystem where T : struct, IParticleData
    {
        protected List<T> particles = new List<T>();

        public override void Tick()
        {
            for (int i = particles.Count - 1; i >= 0; i--)
            {
                if (UpdateParticle(ref particles[i]))
                {
                    OnParticleDestroyed(particles[i]);
                    particles.RemoveAt(i);
                }
            }
        }

        protected abstract bool UpdateParticle(ref T particle);

        protected virtual void OnParticleDestroyed(T particle) { }

        public void Spawn(T particle)
        {
            particles.Add(particle);
        }

        public override void ExposeData()
        {
            Scribe_Collections.Look(ref particles, "particles", LookMode.Deep);
        }
    }
}
