using Verse;

namespace TiberiumRim
{
    public interface IMapWatcher
    {
        public bool IsSpyingNow { get; }
        public Map MapTarget { get; }
    }
}
