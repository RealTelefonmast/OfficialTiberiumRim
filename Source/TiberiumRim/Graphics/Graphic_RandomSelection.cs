using UnityEngine;
using Verse;

namespace TiberiumRim
{ 
    public class Graphic_RandomSelection : Graphic_Collection
    {
        public override Material MatSingle
        {
            get
            {
                return this.subGraphics[Rand.Range(0, this.subGraphics.Length)].MatSingle;
            }
        }

        public Graphic GraphicAt(int i)
        {
            var l = subGraphics.Length;
            i = i % l;
            return subGraphics[i];
        }

        public override Graphic GetColoredVersion(Shader newShader, Color newColor, Color newColorTwo)
        {
            if (newColorTwo != Color.white)
            {
                Log.ErrorOnce("Cannot use Graphic_Random.GetColoredVersion with a non-white colorTwo.", 9910251, false);
            }
            return GraphicDatabase.Get<Graphic_Random>(this.path, newShader, this.drawSize, newColor, Color.white, this.data);
        }
    }
}
