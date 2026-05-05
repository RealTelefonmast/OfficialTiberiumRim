using TeleCore.RoomComponents;
using TeleCore.Types.Utils;
using Verse;

namespace TeleCore.Types.Abstracts;

//TODO: Move to TAE
public abstract class AtmosphereConverter
{
    protected readonly Thing _sourceThing;
    private RoomComponent_Atmosphere _cachedComp;

    public AtmosphereConverter(Thing thing)
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

    public abstract float BurningRate { get; }

    public virtual bool IsActive => Atmosphere.Volume.StoredValueOf(NMODefOf.Atmosphere_Oxygen) >= BurningRate;

    public abstract void Tick();
}