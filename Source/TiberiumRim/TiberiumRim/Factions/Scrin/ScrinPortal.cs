using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using UnityEngine;

namespace TiberiumRim
{
    public class ScrinPortal : PortalSpawner
    {
        protected override int ParticleTick => 20;

        protected override void DoParticleEffect()
        {
            float angleFromCenter = TRUtils.Range(0, 360);
            Vector3 rand = DrawPos + Quaternion.Euler(0, angleFromCenter, 0) * new Vector3(TRUtils.Range(4f, 4.25f), 0, 0);
            float angleToCenter = TRUtils.AngleWrapped(angleFromCenter + 270);

            MoteThrown mote = (MoteThrown)ThingMaker.MakeThing(ThingDef.Named("PortalParticle"), null);
            mote.Scale = TRUtils.Range(0.45f, 0.65f);
            mote.exactPosition = rand;
            mote.SetVelocity(angleToCenter, Rand.Range(1f, 1.25f));
            GenSpawn.Spawn(mote, rand.ToIntVec3(), Map, WipeMode.Vanish);
        }
    }
}
