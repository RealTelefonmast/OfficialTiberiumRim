using System.Collections.Generic;
using System.Xml;
using JetBrains.Annotations;
using RimWorld;
using TeleCore.Atmosphere.Defs;
using TeleCore.Atmosphere.Rooms;
using TeleCore.MapInfo;
using TeleCore.Primitive;
using TeleCore.Utility;
using UnityEngine;
using Verse;

namespace TeleCore.Atmosphere.Oxygen.MapData;

public class AirSourceProperties
{
    public List<DefValue<AtmosphericValueDef, double>> atmospheres;
    public int interval;
}

public class AtmosConversionRule
{
    public AtmosConversionRule from;
    public AtmosConversionRule to;

    public void LoadDataFromXmlCustom(XmlNode root)
    {
    }
}

public class AirSource
{
}

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

    public abstract void Tick();
}

public class OxygenBurner : AtmosphereConverter
{
    public OxygenBurner(ThingWithComps thing) : base(thing)
    {
    }

    public override void Tick()
    {
        if (GenTicks.TicksAbs % 180 != 0) return;
        if (Atmosphere == null)
        {
            Log.Warning($"Tried to tick oxygen burner with thing without a room: {_sourceThing}");
            return;
        }

        var result = Atmosphere.Volume.TryRemove(NMODefOf.Atmosphere_Oxygen, 10d);
        if (result)
        {
            Atmosphere.Volume.TryAdd(NMODefOf.Atmosphere_CarbonMonoxide, 5);
        }
        else
        {
            if (_sourceThing.TryGetComp<CompBreakdownable>(out var comp)) comp.DoBreakdown();
        }
    }
}

public class OxygenBurnerFire : AtmosphereConverter
{
    private readonly Fire _sourceFire;

    public OxygenBurnerFire(Fire fire) : base(fire)
    {
        _sourceFire = fire;
    }

    public override void Tick()
    {
        if (GenTicks.TicksAbs % 2 != 0) return;
        var saturation = Atmosphere.Volume.StoredPercentOf(NMODefOf.Atmosphere_Oxygen);
        var result = Atmosphere.Volume.TryRemove(NMODefOf.Atmosphere_Oxygen, 1d);
        if (result.Actual[NMODefOf.Atmosphere_Oxygen] > 0)
            Atmosphere.Volume.TryAdd(NMODefOf.Atmosphere_CarbonMonoxide, 0.25f);

        _sourceFire.fireSize = Mathf.Clamp(_sourceFire.fireSize, 0, Fire.MaxFireSize * saturation);
        // else
        // {
        //     _sourceFire.TakeDamage(new DamageInfo(DamageDefOf.Extinguish, 10f));
        // }
    }
}

public class AirMapInfo : MapInformation
{
    private static List<AtmosConversionRule> _rules;
    private readonly Dictionary<Thing, AtmosphereConverter> _converters;
    private readonly List<AtmosphereConverter> _convs;

    static AirMapInfo()
    {
    }

    public AirMapInfo([NotNull] Map map) : base(map)
    {
        _convs = new List<AtmosphereConverter>();
        _converters = new Dictionary<Thing, AtmosphereConverter>();
    }

    public IReadOnlyCollection<AtmosphereConverter> Converters => _converters.Values;

    public AtmosphereConverter ConverterFor(Thing thing)
    {
        if (_converters.TryGetValue(thing, out var conv))
            return conv;
        return null;
    }

    public void RegisterBurner(ThingWithComps burner)
    {
        var converter = new OxygenBurner(burner);
        _convs.Add(converter);
        _converters.Add(burner, converter);
    }

    public void RegisterFire(Fire fire)
    {
        var converter = new OxygenBurnerFire(fire);
        _convs.Add(converter);
        _converters.Add(fire, converter);
    }

    public void Deregister(Thing thing)
    {
        if (_converters.TryGetValue(thing, out var converter))
        {
            _convs.Remove(converter);
            _converters.Remove(thing);
        }
    }

    public override void Tick()
    {
        for (var i = _convs.Count - 1; i >= 0; i--)
        {
            var converter = _convs[i];
            converter.Tick();
        }
    }
}