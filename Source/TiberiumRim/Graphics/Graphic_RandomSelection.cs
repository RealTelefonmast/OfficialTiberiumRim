using UnityEngine;
using Verse;

namespace TR.Graphics;

public class Graphic_RandomSelection : Graphic_Collection
{
    public override Material MatSingle => subGraphics[Rand.Range(0, subGraphics.Length)].MatSingle;

    public Graphic GraphicAt(int i)
    {
        var l = subGraphics.Length;
        i = i % l;
        return subGraphics[i];
    }

    public override Graphic GetColoredVersion(Shader newShader, Color newColor, Color newColorTwo)
    {
        if (newColorTwo != Color.white)
            Log.ErrorOnce("Cannot use Graphic_Random.GetColoredVersion with a non-white colorTwo.", 9910251, false);
        return GraphicDatabase.Get<Graphic_Random>(path, newShader, drawSize, newColor, Color.white, data);
    }
}