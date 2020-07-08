using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace TiberiumRim
{
    public class Event_TiberiumArrival : BaseEvent
    {
        protected override float WeightForMap(Map map)
        {
            var baseVal = base.WeightForMap(map);
            if (map.TileInfo.Rivers != null)
                return baseVal * 10;
            return baseVal;
        }

        public override void EventAction()
        {
            base.EventAction();
            SkyfallerValue skyfaller = TiberiumCraterDef();
            Map map = MapForEvent;
            if (LandingSiteFor(skyfaller, map, out IntVec3 cell))
            {
                EventTargets = ThingMaker.MakeThing(skyfaller.innerThing);
                SkyfallerMaker.SpawnSkyfaller(skyfaller.skyfallerDef, EventTargets.PrimaryTarget.Thing, cell, map);
            }
        }

        private SkyfallerValue TiberiumCraterDef()
        {
            List<SkyfallerValue> skyFallers = new List<SkyfallerValue>()
            {
                new SkyfallerValue(TiberiumDefOf.GreenTiberiumMeteorIncoming, TiberiumDefOf.TiberiumCraterGreen,1, 0.66f), 
                new SkyfallerValue(TiberiumDefOf.BlueTiberiumMeteorIncoming, TiberiumDefOf.TiberiumCraterBlue,1, 0.33f), 
                new SkyfallerValue(TiberiumDefOf.GreenTiberiumMeteorIncoming, TiberiumDefOf.TiberiumCraterHybrid,1, 0.22f),
                new SkyfallerValue(TiberiumDefOf.RedTiberiumShardIncoming, TiberiumDefOf.RedTiberiumShard, 1, 0.01f)
            };
            return skyFallers.RandomElementByWeight(s => s.chance);
        }

        private bool LandingSiteFor(SkyfallerValue skyfaller, Map map, out IntVec3 foundCell)
        {
            return CellFinderLoose.TryFindSkyfallerCell(skyfaller.skyfallerDef, map, out foundCell, 20, default(IntVec3), -1, true, true, false, true, false, true, delegate (IntVec3 x)
            {
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
                if (thingList.Any(t => t.def.IsEdifice() || t is TiberiumCrystal)) return false;

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
            });
        }
    }
}
