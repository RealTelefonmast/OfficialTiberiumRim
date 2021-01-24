using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public static class TiberiumMotes
    {
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
