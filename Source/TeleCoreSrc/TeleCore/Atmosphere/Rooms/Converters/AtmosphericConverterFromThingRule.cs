using System.Collections.Generic;
using TeleCore.Logging;
using Verse;

namespace TeleCore.Atmosphere.Rooms.Converters;

public abstract class AtmosphericConverterFromThingRule
{
    internal void ConverterFor(Thing thing, List<AtmosphereConverterBase> results)
    {
        var converter = ConverterFor(thing);
        if (converter != null)
        {
            TLog.Message($"Adding converter for {thing}: {converter.GetType()}");
            results.Add(converter);
        }
    }

    public abstract AtmosphereConverterBase? ConverterFor(Thing thing);
}