using System.Collections.Generic;
using Verse;

namespace TR.TiberiumEnvironment.Conversions;

public class TerrainConversion
{
    public TerrainFilter filter;
    public List<WeightedTerrain> toTerrain;

    public bool Supports(TerrainDef def)
    {
        return filter.Supports(def);
    }

    public TerrainDef RandomOutcome()
    {
        return toTerrain.RandomElementByWeight(w => w.weight).terrainDef;
    }
}