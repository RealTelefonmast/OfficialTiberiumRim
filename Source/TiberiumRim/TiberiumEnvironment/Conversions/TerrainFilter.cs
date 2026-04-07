using System.Collections.Generic;
using TR.Defs;
using TerrainDef = Verse.TerrainDef;

namespace TR.TiberiumEnvironment.Conversions;

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