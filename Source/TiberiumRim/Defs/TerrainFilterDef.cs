using System.Collections.Generic;
using System.Linq;
using Verse;

namespace TR.Defs;

public class TerrainFilterDef : Def
{
    public List<TerrainFilterDef> acceptedFilters;
    public List<string> acceptedTags;
    public List<TerrainDef> acceptedTerrain;
    public List<string> ignoreTags;
    public List<string> neededTags;

    //We first check if there is anything to ignore, if so, we skip it
    //We then check if we have fixed terrain defs, if so, we accept it
    //If it is not accepted by default, we check if any tags overlap, if so, we accept it
    public bool Allows(TerrainDef def)
    {
        if (def == null) return false;
        var name = def.defName.ToLower();
        if (ignoreTags?.Any(name.Contains) ?? false) return false;

        var needsAccepted = !acceptedTags.NullOrEmpty();
        var needsNeeded = !neededTags.NullOrEmpty();
        var needsAcceptedTerr = !acceptedTerrain.NullOrEmpty();
        var needsFilters = !acceptedFilters.NullOrEmpty();

        var acceptedTrue = !needsAccepted || Enumerable.Any(acceptedTags, name.Contains);
        var neededTrue = !needsNeeded || neededTags.All(name.Contains);
        var acceptedTerrTrue = !needsAcceptedTerr || acceptedTerrain.Contains(def);
        var filterTrue = !needsFilters || Enumerable.Any(acceptedFilters, f => f.Allows(def));

        if (!neededTrue) return false;
        if (needsAcceptedTerr)
        {
            if (acceptedTerrTrue) return true;
            if (!needsFilters && !needsAccepted) return false;
        }

        return filterTrue && acceptedTrue;
    }

    // Backward-compat alias
    public bool AllowsTerrainDef(TerrainDef def)
    {
        return Allows(def);
    }
}