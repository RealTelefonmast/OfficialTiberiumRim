using Verse;

namespace TR.GameParts.Interfaces;

public interface IPollutionSource
{
    public Thing Thing { get; }
    public Room Room { get; }
    int PollutionInterval { get; }
    int PollutionAmount { get; }
}