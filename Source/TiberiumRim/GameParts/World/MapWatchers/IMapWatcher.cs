using Verse;

namespace TR.MapWatchers;

public interface IMapWatcher
{
    public bool IsSpyingNow { get; }
    public Map MapTarget { get; }
}