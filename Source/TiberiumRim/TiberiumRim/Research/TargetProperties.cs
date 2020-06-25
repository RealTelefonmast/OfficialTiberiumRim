using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace TiberiumRim
{
    public class TargetProperties
    {
        public Type targetType;
        public List<ThingDef> targetDefs;
        public int distanceFromTarget;
        public string groupLabel;

        public bool Accepts(Thing thing, Thing from = null)
        {
            return thing.def.thingClass == targetType || targetDefs.Contains(thing.def) && from != null ? from.Position.DistanceTo(thing.Position) <= distanceFromTarget : true;
        }

        public Thing FindStation(Map map)
        {
            Thing thing = null;

            void Action(IntVec3 c)
            {
                var list = c.GetThingList(map);
                thing = list.First(t =>
                    t.GetType() == targetType || (targetDefs != null && targetDefs.Contains(t.def)));
            }

            bool Predicate(IntVec3 c) => c.IsValid && thing == null;
            var pawns = map.mapPawns.FreeColonistsSpawned;
            map.floodFiller.FloodFill(pawns.FirstOrDefault().Position, Predicate, Action, default, false,
                pawns.Select(p => p.Position));
            return thing;
        }
    }
}
