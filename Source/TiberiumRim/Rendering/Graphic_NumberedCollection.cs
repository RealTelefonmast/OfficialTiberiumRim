using Verse;

namespace TR
{
    public class Graphic_NumberedCollection : Graphic_Collection
    {
        public override void Init(GraphicRequest req)
        {
            base.Init(req);
        }

        public int Count => subGraphics.Length;

        public Graphic[] Graphics => subGraphics;
    }
}
