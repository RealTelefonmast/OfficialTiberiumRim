using TeleCore.Defs.Extensions;
using TeleCore.Events;
using TeleCore.Static;
using TeleCore.Utility;
using Verse;

namespace TeleCore.ThingComps;

/// <summary>
///     Base class for TeleCore ThingComps.
/// </summary>
public interface ITeleComp
{
    void CustomTick(float tickRate);
}

public abstract class TeleComp : ThingComp
{
    public TeleDefExtension Extension { get; private set; }

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);
        if (parent.def.HasTeleExtension(out var textension))
        {
            Extension = textension;
            if (Extension.addCustomTick)
                TeleEventHandler.EntityTicked += TeleTick;
        }
    }

    public override void PostDeSpawn(Verse.Map map)
    {
        base.PostDeSpawn(map);
        if (Extension != null)
            if (Extension.addCustomTick && !parent.IsTeleEntity())
                TeleEventHandler.EntityTicked -= TeleTick;
    }

    internal virtual void TeleTick()
    {
    }
}