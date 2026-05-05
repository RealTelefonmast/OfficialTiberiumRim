using System.Collections.Generic;
using TeleCore.Types.Exposables;
using Verse;

namespace TeleCore.Defs;

/// <summary>
///     Allows you to set biome-wide atmospheres.
/// </summary>
public class TAE_BiomeExtension : DefModExtension
{
    public List<DefValueLoadable<AtmosphericValueDef, float>> uniqueAtmospheres;
}