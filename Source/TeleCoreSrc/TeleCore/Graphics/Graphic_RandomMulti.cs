using Verse;

namespace TeleCore;

public class Graphic_RandomMulti : Graphic
{
    public const string NorthSuffix = "_north";
    public const string SouthSuffix = "_south";
    public const string EastSuffix = "_east";
    public const string WestSuffix = "_west";
    public const string MaskSuffix = "_m";
    private Graphic_Multi[] subGraphics;

    public override void Init(GraphicRequest req)
    {
        data = req.graphicData;
        path = req.path;
        maskPath = req.maskPath;
        color = req.color;
        colorTwo = req.colorTwo;
        drawSize = req.drawSize;
    }
}