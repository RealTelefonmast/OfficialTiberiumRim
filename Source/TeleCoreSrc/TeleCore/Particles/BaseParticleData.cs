using TeleCore.Defs;
using UnityEngine;
using Verse;

namespace TeleCore.Particles
{
    public struct BaseParticleData : IParticleData
    {
        public ParticleDef def;
        public Vector3 pos;
        public float rotation;
        public float scale;
        public int age;
        public Vector3 velocity;

        public void ExposeData()
        {
            Scribe_Defs.Look(ref def, "def");
            Scribe_Values.Look(ref pos, "pos");
            Scribe_Values.Look(ref rotation, "rotation");
            Scribe_Values.Look(ref scale, "scale");
            Scribe_Values.Look(ref age, "age");
            Scribe_Values.Look(ref velocity, "velocity");
        }
    }
}
