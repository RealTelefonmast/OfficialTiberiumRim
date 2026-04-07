using TR.Defs;
using TR.Util;
using Verse;

namespace TR.TiberiumEnvironment;

[StaticConstructorOnStartup]
public class TiberiumMapRenderer
{
    public TiberiumFieldFogLayer[] fogLayers;
    public Map map;

    public TiberiumMapRenderer(Map map)
    {
        this.map = map;
        var tiberium = map.Tiberium();
        var grids = tiberium.TiberiumInfo.TiberiumGrid;
        fogLayers = new TiberiumFieldFogLayer[3]
        {
            new(MainTCD.Main.GreenColor, grids.fieldColorGrids[0]),
            new(MainTCD.Main.BlueColor, grids.fieldColorGrids[1]),
            new(MainTCD.Main.RedColor, grids.fieldColorGrids[2])
        };
    }

    public void DrawAllTiberiumLayers()
    {
        foreach (var fogLayer in fogLayers) fogLayer.DrawFieldFog(map);
    }
}