using RimWorld;
using TeleCore.MapComponents;
using TeleCore.Types.Utils;
using Verse;

namespace TeleCore.Types;

public class OxygenBurner : AtmosphereConverter
{
    private readonly CompRefuelable _fuelSource;

    private bool previouslyHadAtmosphere;

    public OxygenBurner(ThingWithComps thing) : base(thing)
    {
        _fuelSource = thing.GetComp<CompRefuelable>();
    }

    public override float BurningRate => 10;

    public override void Tick()
    {
        if (GenTicks.TicksAbs % 90 != 0) return;
        if (Atmosphere == null)
        {
            Log.Warning($"Tried to tick oxygen burner with thing without a room: {_sourceThing}");
            return;
        }

        if (!_fuelSource.HasFuel) return;

        var result = Atmosphere.Volume.TryRemove(NMODefOf.Atmosphere_Oxygen, BurningRate);
        if (result)
        {
            previouslyHadAtmosphere = true;
            Atmosphere.Volume.TryAdd(NMODefOf.Atmosphere_CarbonMonoxide, 5);
        }
        else if (previouslyHadAtmosphere)
        {
            previouslyHadAtmosphere = false;
            ((ThingWithComps)_sourceThing).BroadcastCompSignal(KnownCompSignals.RanOutOfFuel);
        }
    }
}