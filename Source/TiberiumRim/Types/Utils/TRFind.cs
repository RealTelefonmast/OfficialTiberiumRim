using TR.GameUpdate;
using TR.World;

namespace TR;

public static class TRFind
{
    public static TiberiumRoot TRoot { get; set; }

    public static TiberiumTickManager TickManager => TRoot.TickManager;
    public static PlanetLayer_Tiberium CurPlanetLayer { get; }
}