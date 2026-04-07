using System.Collections.Generic;
using TeleCore.Defs.Values;
using Verse;

namespace TeleCore.Atmosphere.Defs;

/// <summary>
///     Allows you to set biome-wide atmospheres.
/// </summary>
public class TAE_BiomeExtension : DefModExtension
{
    public List<DefValueLoadable<AtmosphericValueDef, float>> uniqueAtmospheres;
}