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

        private float exactScale = 1f;
        private float exactRotation = 0f;

        private Vector3 exactPos;
        private Vector3 scaleVector = Vector3.one;
        private IntVec3 startCell = IntVec3.Invalid;
        private IntVec3 endCell = IntVec3.Invalid;


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

        public float ExactRotation => exactRotation;

        public Vector3 ExactScale => scaleVector;

        public Vector3 Position => exactPos;

        public Graphic Graphic => graphicInt ??= def.graphicData.Graphic;

        public void Draw()
        {

        }
    }
}
