using Verse;

namespace TR.Info;

public class UndergroundMapInfo : MapInformation
{
    public UndergroundMapInfo(Map map) : base(map)
    {
    }

    public override void ExposeDataExtra()
    {
    }
}

public class UndergroundResourceLayer : IExposable
{
    private int layerDepth;

    public void ExposeData()
    {
    }
}

public class UndergroundResourceDef : Def
{
}