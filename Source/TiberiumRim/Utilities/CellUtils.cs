using System.Linq;
using RimWorld;
using Verse;

namespace TiberiumRim
{
    public static class CellUtils
    {
        public static bool AllowTiberiumMeteorite(IntVec3 x, Map map)
        {
            //Check if the searched pos is even viable
            if (map.Tiberium().NaturalTiberiumStructureInfo.AllProducers.Any(p => p.Position.DistanceTo(x) < 30)) return false;
            //If pos is viable, check if all needed surrounding cells are viable too
            foreach (var c in GenAdj.CellsOccupiedBy(x, Rot4.North, new IntVec2(5, 5)))
            {
                if (!c.InBounds(map)) return false;
                if (c.InNoBuildEdgeArea(map)) return false;
                if (c.Fogged(map)) return false;
                //if (!c.Standable(map)) return false;

                //Terrain needs to support research crane
                TerrainDef terrain = c.GetTerrain(map);
                if (!terrain.affordances.Contains(TiberiumDefOf.TiberiumResearchCrane.terrainAffordanceNeeded)) return false;
                if (terrain.affordances.Contains(TerrainAffordanceDefOf.Bridgeable)) return false;

                //Prefer near river positions
                if (!map.TileInfo.Rivers.NullOrEmpty())
                {
                    var TibWaterComp = map.Tiberium().TerrainInfo.WaterInfo;
                    if (!TibWaterComp.landableCells[x]) return false;
                }
            }
            return true;
        }
    }
}
