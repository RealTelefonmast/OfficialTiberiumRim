using TR.GameParts.GameUpdate;
using TR.TiberiumEnvironment.World;

namespace TR.Util;

public static class TRFind
{
    public static TiberiumRoot TRoot { get; set; }

    public static TiberiumTickManager TickManager => TRoot.TickManager;
    public static PlanetLayer_Tiberium CurPlanetLayer { get; }
}