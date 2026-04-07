using TeleCore;
using TeleCore.Utility;
using Verse;

namespace TeleCore.Atmosphere.Rooms.Converters;

public abstract class AtmosphereConverterBase
{
    private RoomComponent_Atmosphere _cachedComp;
    protected readonly Thing _sourceThing;

    protected RoomComponent_Atmosphere Atmosphere
    {
        get
        {
            if (_cachedComp == null || _cachedComp.Disbanded)
                _cachedComp = _sourceThing?.GetRoom()?.GetRoomComp<RoomComponent_Atmosphere>();
            return _cachedComp;
        }
    }

    public abstract bool IsActive { get; }

    public AtmosphereConverterBase(Thing thing)
    {
        _sourceThing = thing;
    }

    internal void TickInternal()
    {
        if (Atmosphere == null)
        {
            global::TeleCore.Logging.TLog.Warning($"Tried to tick converter with thing without a room: {_sourceThing}");
            return;
        }

        if (IsActive)
        {
            Tick();
        }
    }

    public abstract void Tick();
}
