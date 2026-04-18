using Verse;

namespace TeleCore.Unsorted;

public abstract class AtmosphereConverterBase
{
    protected readonly Thing _sourceThing;
    private RoomComponent_Atmosphere _cachedComp;

    public AtmosphereConverterBase(Thing thing)
    {
        _sourceThing = thing;
    }

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

    internal void TickInternal()
    {
        if (Atmosphere == null)
        {
            TLog.Warning($"Tried to tick converter with thing without a room: {_sourceThing}");
            return;
        }

        if (IsActive) Tick();
    }

    public abstract void Tick();
}
