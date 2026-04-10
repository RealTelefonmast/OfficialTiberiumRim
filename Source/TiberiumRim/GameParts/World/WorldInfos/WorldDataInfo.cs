using System.Collections.Generic;
using RimWorld.Planet;
using TR.MapWatchers;
using Verse;

namespace TR.WorldInfos;

public class WorldDataInfo : WorldInformation
{
    //Map Spiers
    private readonly List<IMapWatcher> _mapWatchers = new();

    public WorldDataInfo(World world) : base(world)
    {
    }

    public override void Notify_RegisterWorldObject(GlobalTargetInfo worldObjectOrThing)
    {
        RegisterMapWatcher(worldObjectOrThing);
    }

    private void RegisterMapWatcher(GlobalTargetInfo source)
    {
        IMapWatcher watcher = null;
        if (source.HasThing)
            watcher = source.Thing as IMapWatcher;
        if (source.HasWorldObject)
            watcher = source.WorldObject as IMapWatcher;
        if (watcher != null)
            _mapWatchers.Add(watcher);
    }

    public bool IsSpiedOn(Map map)
    {
        return _mapWatchers.Any(t => t.IsSpyingNow && t.MapTarget == map);
    }
}