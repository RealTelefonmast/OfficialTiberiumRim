using System.Collections.Generic;
using Verse;

namespace TR.Conversions;

public class TerrainConversion
{
    public TerrainFilter filter;
    public List<DefValue<TerrainDef>> toTerrain;

    public bool Supports(TerrainDef def)
    {
        return filter.Supports(def);
    }

    public TerrainDef RandomOutcome()
    {
        return toTerrain.RandomElementByWeight(w => w.Value).Def;
    }
}