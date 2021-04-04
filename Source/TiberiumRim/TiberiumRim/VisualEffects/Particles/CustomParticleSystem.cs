using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class CustomParticleSystem
    {
        public GameObject holder;
        public ParticleSystem system;

        public CustomParticleSystem()
        {
            holder = new GameObject("ParticleHolder");
            holder.AddComponent(typeof(ParticleSystem));
            system = holder.GetComponent<ParticleSystem>();
        }

        public void TestGeneric(Vector3 drawPos)
        {
            var trans = system.transform;
            trans.position = drawPos;

            var em = system.emission;
            em.rateOverTime = new ParticleSystem.MinMaxCurve(300);

            var shape = system.shape;
            shape.shapeType = ParticleSystemShapeType.Rectangle;
            shape.scale = new Vector3(20,20,1);

            var color = system.colorOverLifetime;
            color.color = new ParticleSystem.MinMaxGradient(Color.white, Color.green);

            var size = system.sizeOverLifetime;
            size.size = new ParticleSystem.MinMaxCurve(0, 1);
            
            var renderer = system.GetComponent(typeof(Renderer));
            Log.Message("Renderer exists: " + (renderer != null));

            system.Play();
        }

        public void ConfigureMain()
        {
            var main = system.main;
            //main.duration = ;
            //main.cullingMode = ;
            //main.customSimulationSpace = ;
            //main.emitterVelocityMode = ;
            //main.flipRotation = ;
        }

        public void ConfigureColorOverLifetime()
        {
            var color = system.colorOverLifetime;
        }

        public void AdjustShape()
        {
            var shape = system.shape;
        }

        public void Adjust()
        {
        }
    }
}
