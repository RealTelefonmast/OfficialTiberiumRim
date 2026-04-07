using TR.GameParts.Interfaces;
using TR.Util;
using Verse;

namespace TR.Comps;

public class Comp_PollutionSource : ThingComp, IPollutionSource
{
    public CompProperties_PollutionSource Props => (CompProperties_PollutionSource)props;
    public Thing Thing => parent;
    public Room Room => parent.GetRoomIndirect();
    public int PollutionInterval => Props.pollutionInterval;
    public int PollutionAmount => Props.pollutionAmount;

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);
        Thing.Map.Tiberium().PollutionInfo.RegisterSource(this);
    }

    public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
    {
        base.PostDeSpawn(map);
        Thing.Map.Tiberium().PollutionInfo.DeregisterSource(this);
    }
}

public class CompProperties_PollutionSource : CompProperties
{
    public int pollutionAmount;
    public int pollutionInterval;

    public CompProperties_PollutionSource()
    {
        compClass = typeof(Comp_PollutionSource);
    }
}