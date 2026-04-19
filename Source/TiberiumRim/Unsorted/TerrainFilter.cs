using System.Collections.Generic;
using TerrainDef = Verse.TerrainDef;

namespace TR.Conversions;

public class TerrainFilter
{
    public TerrainFilterDef filterDef;
    public List<TerrainDef> terrainDefs;

    public bool Supports(TerrainDef def)
    {
        if (filterDef != null && filterDef.Allows(def))
            return true;
        return terrainDefs?.Contains(def) ?? false;
    }
}