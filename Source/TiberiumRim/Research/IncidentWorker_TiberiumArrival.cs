using System.Linq;
using RimWorld;
using TR.Components;
using TR.Util;
using Verse;

namespace TR.Research;

public class IncidentWorker_TiberiumArrival : IncidentWorker
{
    private ThingDef innerThing;

    public TiberiumIncidentDef Def => def as TiberiumIncidentDef;

    protected override bool TryExecuteWorker(IncidentParms parms)
    {
        var map = (Map)parms.target;
        if (!CanFireNowSub(parms)) return false;
        var pair = Def.skyfallers.RandomElementByWeight(s => s.chance);
        innerThing = pair.innerThing;
        if (!TryFindCell(out var cell, map)) return false;
        var faller = SkyfallerMaker.MakeSkyfaller(pair.skyfallerDef, pair.innerThing);
        GenSpawn.Spawn(faller, cell, map);
        SendStandardLetter(parms, faller.innerContainer[0]);
        return true;
    }

    protected override bool CanFireNowSub(IncidentParms parms)
    {
        return base.CanFireNowSub(parms);
    }

    private bool TryFindCell(out IntVec3 cell, Map map)
    {
        return CellFinderLoose.TryFindSkyfallerCell(innerThing, map, out cell, 20, default(IntVec3), -1, true, true,
            false, false, false, false, delegate(IntVec3 x)
            {
                if (!x.InBounds(map)) return false;
                if (x.Fogged(map)) return false;
                if (x.InNoBuildEdgeArea(map)) return false;
                var crystal = x.GetTiberium(map);
                if (crystal != null) return false;

                var terrain = x.GetTerrain(map);
                if (terrain == null) return false;
                if (terrain.IsStone()) return false;
                if (terrain.affordances.Contains(DefDatabase<TerrainAffordanceDef>.GetNamed("Bridgeable")))
                    return false;
                bool tryRiver = !map.TileInfo.Rivers.NullOrEmpty();
                var river = map.GetComponent<MapComponent_TiberiumWater>();
                if (tryRiver && (!river.riverCells.ActiveCells.Any(c => c.InHorDistOf(x, 10f)) ||
                                 river.riverCells.ActiveCells.Any(c => c.DistanceTo(x) < 5f)))
                    return false;
                var things = map.listerThings.ThingsOfDef(innerThing);
                float min = 99;
                if (!things.NullOrEmpty())
                    min = map.listerThings.ThingsOfDef(innerThing).Min(d => d.Position.DistanceTo(x));
                if (min < 46f) return false;

                return GenAdj.CellsOccupiedBy(x, Rot4.North, new IntVec2(4, 4)).All(c => c.Standable(map));
            });
    }
}