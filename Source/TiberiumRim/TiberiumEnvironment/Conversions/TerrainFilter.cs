using System.Collections.Generic;
using System.Linq;
using Verse;

namespace TiberiumRim
{
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

    public class TerrainFilterDef : Def
    {
        public List<TerrainFilterDef> acceptedFilters;
        public List<TerrainDef> acceptedTerrain;
        public List<string> acceptedTags;
        public List<string> neededTags;
        public List<string> ignoreTags;

        //We first check if there is anythinh to ignore, if so, we skip it
        //We then check if we have fixed terrain defs, if so, we accept it
        //If it is not accepted by default, we check if any tags overlap, if so, we accept it
        public bool Allows(TerrainDef def)
        {
            if (def == null) return false;
            var name = def.defName.ToLower();
            if (ignoreTags?.Any(t => name.Contains(t)) ?? false) return false;

            var needsAccepted    = !acceptedTags.NullOrEmpty();
            var needsNeeded      = !neededTags.NullOrEmpty();
            var needsAcceptedTerr= !acceptedTerrain.NullOrEmpty();
            var needsFilters     = !acceptedFilters.NullOrEmpty();

            var acceptedTrue     = !needsAccepted || acceptedTags.Any(t => name.Contains(t)); 
            var neededTrue       = !needsNeeded   || neededTags.All(t => name.Contains(t));
            var acceptedTerrTrue = !needsAcceptedTerr || acceptedTerrain.Contains(def); 
            var filterTrue       = !needsFilters  || acceptedFilters.Any(f => f.Allows(def));

            if (!neededTrue) return false;
            if (needsAcceptedTerr)
            {
                if (acceptedTerrTrue) return true;
                if(!needsFilters && !needsAccepted) return false;
            }
            return filterTrue && acceptedTrue;
        }
    }
}
