using Verse;

namespace TeleCore.Graphics;

public class Graphic_NumberedCollection : Graphic_Collection
{
    public int Count => subGraphics.Length;

    public Graphic[] Graphics => subGraphics;

    public override void Init(GraphicRequest req)
    {
        base.Init(req);
    }
}