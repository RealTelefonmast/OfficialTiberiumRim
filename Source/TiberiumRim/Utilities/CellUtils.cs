using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        /*
        if (x.Fogged(map)) return false;
        if (x.InNoBuildEdgeArea(map)) return false;
        //if (!x.Standable(map)) return false;

        //Check acceptable terrain
        TerrainDef terrain = x.GetTerrain(map);
        if (terrain.driesTo != null) return false;
        if (terrain.IsStone()) return false;
        if (terrain.affordances.Contains(DefDatabase<TerrainAffordanceDef>.GetNamed("Bridgeable"))) return false;

        //Check blocking things
        var thingList = x.GetThingList(map);
        if (thingList.Any(t => t.props.IsEdifice() || t is TiberiumCrystal)) return false;

        //Try prefer river cells
        bool tryRiver = !map.TileInfo.Rivers.NullOrEmpty();
        if (tryRiver)
        {
            MapComponent_TiberiumWater river = map.GetComponent<MapComponent_TiberiumWater>();
            if (!river.landableCells[x]) return false;
        }

        //Min Distance From Other Craters
        List<Thing> things = map.listerThings.ThingsOfDef(skyfaller.innerThing);
        float min = 99;
        if (!things.NullOrEmpty())
            min = things.Min(d => d.Position.DistanceTo(x));
        if (min < 46f) return false;

        return GenAdj.CellsOccupiedBy(x, Rot4.North, new IntVec2(4, 4)).All(c => c.Standable(map));
        */
    }
}
