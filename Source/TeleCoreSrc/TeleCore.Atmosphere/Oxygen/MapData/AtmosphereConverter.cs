using TeleCore;
using TeleCore.Atmosphere.Rooms;
using TeleCore.Utility;
using Verse;

namespace TeleCore.Atmosphere.Oxygen.MapData;

//TODO: Move to TAE
public abstract class AtmosphereConverter
{
    private RoomComponent_Atmosphere _cachedComp;
    protected readonly Thing _sourceThing;

    protected RoomComponent_Atmosphere Atmosphere
    {
        get
        {
            if (_cachedComp == null || _cachedComp.Disbanded)
            {
                _cachedComp = _sourceThing?.GetRoom()?.GetRoomComp<RoomComponent_Atmosphere>();
            }
            return _cachedComp;
        }
    }

    public abstract float BurningRate { get; }

    public virtual bool IsActive => Atmosphere.Volume.StoredValueOf(NMODefOf.Atmosphere_Oxygen) >= BurningRate;

    public AtmosphereConverter(Thing thing)
    {
        _sourceThing = thing;
    }

    public abstract void Tick();
}
