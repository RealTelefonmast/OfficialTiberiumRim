using Verse;

namespace TR.Interfaces;

public interface IPollutionSource
{
    public Thing Thing { get; }
    public Room Room { get; }
    int PollutionInterval { get; }
    int PollutionAmount { get; }
}