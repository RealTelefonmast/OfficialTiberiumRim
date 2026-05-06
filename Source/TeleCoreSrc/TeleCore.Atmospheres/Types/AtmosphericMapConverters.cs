using System;
using System.Collections.Generic;
using System.Linq;
using TeleCore.Types.Abstracts;
using TeleCore.Types.Structs;
using TeleCore.Types.Utils;
using Verse;

namespace TeleCore.Atmospheres.Types;

internal delegate void AtmosphereConverterEventInternal(Thing thing, List<AtmosphereConverterBase> results);

public class AtmosphericMapConverters
{
    private static readonly bool _isActive;
    private readonly Dictionary<Thing, List<AtmosphereConverterBase>> _converters;

    public AtmosphericMapConverters()
    {
        _converters = new Dictionary<Thing, List<AtmosphereConverterBase>>();
        GlobalEventHandler.ThingSpawned += Notify_ThingSpawned;
        GlobalEventHandler.ThingDespawning += Notify_ThingDespawned;
    }

    public IEnumerable<AtmosphereConverterBase> Converters => _converters.SelectMany(v => v.Value);

    private void Notify_ThingSpawned(ThingStateChangedEventArgs args)
    {
        if (!_isActive) return;

        var thing = args.Thing;
        _converters.Add(thing, GenerateConvertersFor(thing));
    }

    private void Notify_ThingDespawned(ThingStateChangedEventArgs args)
    {
        var thing = args.Thing;
        if (_converters.ContainsKey(thing)) _converters.Remove(thing);
    }

    public void Tick()
    {
        if (!_isActive) return;
        foreach (var converter in Converters)
            converter.Tick();
    }

    public List<AtmosphereConverterBase> ConvertersFor(Thing thing)
    {
        if (_converters.TryGetValue(thing, out var converters))
            return converters;
        return null;
    }

    public T ConverterFor<T>(Thing thing) where T : AtmosphereConverterBase
    {
        if (_converters.TryGetValue(thing, out var converters))
            foreach (var conv in converters)
                if (conv is T t)
                    return t;
        return null;
    }

    #region Factory

    private static event AtmosphereConverterEventInternal ConverterFrom;

    static AtmosphericMapConverters()
    {
        var types = typeof(AtmosphericConverterFromThingRule).AllSubclassesNonAbstract();
        if (types.NullOrEmpty())
        {
            _isActive = false;
            return;
        }

        //
        foreach (var type in types)
        {
            var instance = (AtmosphericConverterFromThingRule)Activator.CreateInstance(type);
            ConverterFrom += instance.ConverterFor;
        }

        _isActive = true;
    }

    private static List<AtmosphereConverterBase> GenerateConvertersFor(Thing thing)
    {
        var results = new List<AtmosphereConverterBase>();
        ConverterFrom.Invoke(thing, results);
        return results;
    }

    #endregion
}