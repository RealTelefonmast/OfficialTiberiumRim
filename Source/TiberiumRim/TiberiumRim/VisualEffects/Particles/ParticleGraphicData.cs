using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using UnityEngine;

namespace TiberiumRim
{
    public class ParticleGraphicData : GraphicData
    {
        public bool randomTexture = false;

        public Graphic GraphicColoredFor(Particle p, Color color, Color colorTwo)
        {
            if (color.IndistinguishableFrom(this.Graphic.Color) && colorTwo.IndistinguishableFrom(this.Graphic.ColorTwo))
            {
                return this.Graphic;
            }
            return this.Graphic.GetColoredVersion(this.Graphic.Shader, color, colorTwo);
        }
    }
}
