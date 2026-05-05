using UnityEngine;
using Verse;

namespace TeleCore.Rendering.Particles;

public class ParticleGraphicData : GraphicData
{
    public bool randomTexture = false;

    public Graphic GraphicColoredFor(Particle p, Color color, Color colorTwo)
    {
        if (color.IndistinguishableFrom(Graphic.Color) && colorTwo.IndistinguishableFrom(Graphic.ColorTwo))
            return Graphic;
        return Graphic.GetColoredVersion(Graphic.Shader, color, colorTwo);
    }
}