using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace TiberiumRim
{
    public class Event_TiberiumArrival : BaseEvent
    {
        public override void EventAction()
        {
            base.EventAction();
            SkyfallerValue skyfaller = TiberiumCraterDef();
            Map map = MapForEvent;
            Log.Message("Should spawn Meteor Now!" + skyfaller.innerThing);
            if (LandingSiteFor(skyfaller, map, out IntVec3 cell))
            {
                var faller = SkyfallerMaker.MakeSkyfaller(skyfaller.skyfallerDef, skyfaller.innerThing);
                GenSpawn.Spawn(faller, cell, map);
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
                //if (x.GetTiberium(map) != null) return false;
                TerrainDef terrain = x.GetTerrain(map);
                if (terrain.driesTo != null)
                    return false;

                bool tryRiver = !map.TileInfo.Rivers.NullOrEmpty();
                MapComponent_TiberiumWater river = map.GetComponent<MapComponent_TiberiumWater>();
                if (tryRiver && river.LandableCells.Contains(x))
                    return false;

                //Min Distance From Other Craters
                List<Thing> things = map.listerThings.ThingsOfDef(skyfaller.innerThing);
                float min = 99;
                if (!things.NullOrEmpty())
                    min = things.Min(d => d.Position.DistanceTo(x));
                if (min < 46f) return false;

                return true;

            });
        }
    }
}
