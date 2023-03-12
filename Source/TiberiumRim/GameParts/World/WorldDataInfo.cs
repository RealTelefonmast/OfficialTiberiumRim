using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace TiberiumRim
{
    public class WorldDataInfo : WorldInfo
    {
        //Map Spiers
        private List<IMapWatcher> mapWatchers = new List<IMapWatcher>();

        public WorldDataInfo(World world) : base(world)
        {
        }

        public void RegisterMapWatcher(GlobalTargetInfo source)
        {
            IMapWatcher watcher = null;
            if (source.HasThing)
                watcher = source.Thing as IMapWatcher;
            if (source.HasWorldObject)
                watcher = source.WorldObject as IMapWatcher;
            if(watcher != null)
                mapWatchers.Add(watcher);
        }

        public bool IsSpiedOn(Map map)
        {
            return mapWatchers.Any(t => t.IsSpyingNow && t.MapTarget == map);
        }
    }
}
