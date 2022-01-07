using System.Collections.Generic;
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

        private Skyfaller skyfaller;
        private LocalTargetInfo targetInfo = LocalTargetInfo.Invalid;

        public override void EventSetup()
        {
            if (LandingSiteFor(TiberiumDefOf.TiberiumMeteorIncoming, MapForEvent, out IntVec3 cell))
            {
                targetInfo = cell;
                EventTargets = new LookTargets(cell, MapForEvent);
                skyfaller = SkyfallerMaker.MakeSkyfaller(TiberiumDefOf.TiberiumMeteorIncoming, ThingMaker.MakeThing(TiberiumDefOf.TiberiumMeteoriteChunk));
            }
        }

        public override bool CanDoEventAction(int curTick)
        {
            return base.CanDoEventAction(curTick);
        }

        public override void EventAction()
        {
            Map map = MapForEvent;
            if (targetInfo.IsValid)
            {
                GenSpawn.Spawn(skyfaller, targetInfo.Cell, map);
            }
        }

        private SkyfallerValue TiberiumCraterDef()
        {
            return new SkyfallerValue(TiberiumDefOf.TiberiumMeteorIncoming, TiberiumDefOf.TiberiumMeteoriteChunk);
            List<SkyfallerValue> skyFallers = new List<SkyfallerValue>()
            {
                new SkyfallerValue(TiberiumDefOf.GreenTiberiumMeteorIncoming, TiberiumDefOf.TiberiumCraterGreen,1, 0.66f), 
                new SkyfallerValue(TiberiumDefOf.BlueTiberiumMeteorIncoming, TiberiumDefOf.TiberiumCraterBlue,1, 0.33f), 
                new SkyfallerValue(TiberiumDefOf.GreenTiberiumMeteorIncoming, TiberiumDefOf.TiberiumCraterHybrid,1, 0.22f),
                new SkyfallerValue(TiberiumDefOf.RedTiberiumShardIncoming, TiberiumDefOf.RedTiberiumShard, 1, 0.01f)
            };
            return skyFallers.RandomElementByWeight(s => s.chance);
        }

        private bool LandingSiteFor(ThingDef skyfaller, Map map, out IntVec3 pos)
        {
            return CellFinderLoose.TryFindSkyfallerCell(skyfaller, map, out pos, 10, map.Center, 999999, true, true,
                false, false, false, false, x => CellUtils.AllowTiberiumMeteorite(x, map));
        }
    }
}
