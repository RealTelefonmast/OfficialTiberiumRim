using Verse;

namespace TR.GameParts.MapWatchers;

public interface IMapWatcher
{
    public bool IsSpyingNow { get; }
    public Map MapTarget { get; }
}