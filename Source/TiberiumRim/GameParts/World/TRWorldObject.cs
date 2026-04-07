using RimWorld.Planet;
using TR.Util;

namespace TR.GameParts;

public class TRWorldObject : WorldObject
{
    public override void SpawnSetup()
    {
        TRUtils.Tiberium().Notify_RegisterWorldObject(this);
        base.SpawnSetup();
    }
}