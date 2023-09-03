using Verse;

namespace TR
{
    public interface IMapWatcher
    {
        public bool IsSpyingNow { get; }
        public Map MapTarget { get; }
    }
}
