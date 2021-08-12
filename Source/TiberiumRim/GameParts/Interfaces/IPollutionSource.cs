using Verse;

namespace TiberiumRim
{
    public interface IPollutionSource
    {
        public Thing Thing { get; }
        public Room Room { get; }
        bool IsPolluting { get; }
        int PollutionInterval { get; }
        int PollutionAmount { get; }
    }
}
