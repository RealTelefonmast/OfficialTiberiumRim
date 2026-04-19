using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace TiberiumRim;

public class TargetProperties
{
    public int distanceFromTarget;
    public string groupLabel;
    public List<ThingDef> targetDefs;
    public Type targetType;

    public Thing FindStation(Map map)
    {
        Thing thing = null;

        void Action(IntVec3 c)
        {
            var list = c.GetThingList(map);
            thing = list.First(t =>
                t.GetType() == targetType || (targetDefs != null && targetDefs.Contains(t.def)));
        }

        bool Predicate(IntVec3 c)
        {
            return c.IsValid && thing == null;
        }

        var pawns = map.mapPawns.FreeColonistsSpawned;
        map.floodFiller.FloodFill(pawns.FirstOrDefault().Position, Predicate, Action, default, false,
            pawns.Select(p => p.Position));
        return thing;
    }
}