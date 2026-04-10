using RimWorld.Planet;

namespace TR;

public class TRWorldObject : WorldObject
{
    public override void SpawnSetup()
    {
        TRUtils.Tiberium().Notify_RegisterWorldObject(this);
        base.SpawnSetup();
    }
}