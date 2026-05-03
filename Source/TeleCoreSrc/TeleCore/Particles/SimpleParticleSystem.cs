using UnityEngine;
using Verse;

namespace TeleCore.Particles
{
    public class SimpleParticleSystem : ParticleSystemBase<BaseParticleData>
    {
        public override void Draw()
        {
            foreach (var particle in particles)
            {
                // Basic drawing logic
                // In a real implementation, we'd use DrawBatch or similar
                // For now, let's just use the particle's graphic
                if (particle.def.graphicData != null)
                {
                    Graphic graphic = particle.def.graphicData.Graphic;
                    // graphic.Draw(particle.pos, Rot4.North, null, particle.rotation);
                    // Note: Real implementation would need to handle altitude, color, etc.
                }
            }
        }

        protected override bool UpdateParticle(ref BaseParticleData particle)
        {
            particle.age++;
            particle.pos += particle.velocity;
            
            // Check if it should be destroyed (e.g. based on lifeTicks in Def)
            // For now, let's just use a fixed age
            if (particle.age > 100) return true; 

            return false;
        }
    }
}
