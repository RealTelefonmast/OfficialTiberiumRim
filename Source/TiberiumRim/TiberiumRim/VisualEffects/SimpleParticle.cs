using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class SimpleParticle
    {
        public ParticleDef def;

        private Graphic graphicInt;
        private Vector3 exactPos;

        public SimpleParticle() { }

        public SimpleParticle(ParticleDef def)
        {
            this.def = def;
        }

        public void SpawnSetup()
        {

        }

        public virtual void Tick()
        {

        }

        public Vector3 Position => exactPos;

        public Graphic Graphic => graphicInt ??= def.graphicData.Graphic;

        public void Draw()
        {

        }
    }
}
