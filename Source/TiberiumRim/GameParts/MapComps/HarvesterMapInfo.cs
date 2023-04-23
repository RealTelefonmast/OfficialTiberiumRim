using System.Collections.Generic;
using System.Linq;
using RimWorld;
using TeleCore;
using Verse;
using Verse.AI;

namespace TiberiumRim
{
    public class HarvesterMapInfo : MapInformation
    {
        public List<Harvester> AllHarvesters = new List<Harvester>();

        private MapComponent_Tiberium Tiberium => map.Tiberium();

        private TiberiumGrid TiberiumGrid => Tiberium.TiberiumInfo.TiberiumGrid;

        public HarvesterMapInfo(Map map) : base(map)
        {
            //harvestableBools = new BoolGrid(map);
        }

        public override void ExposeDataExtra()
        {
        }

        public override void InfoInit(bool initAfterReload = false)
        {
            base.InfoInit(initAfterReload);
            //ReservedTypes.Add(HarvestType.Valuable, 0);
            //ReservedTypes.Add(HarvestType.Unvaluable, 0);
        }

        public void RegisterHarvester(Harvester harvester)
        {
            AllHarvesters.Add(harvester);
        }

        public void DeregisterHarvester(Harvester harvester)
        {
            AllHarvesters.Add(harvester);
        }

        private bool RegionHasTiberium(Region region)
        {
            return TiberiumCrystalsInRegion(region).Any(c => c.HarvestableNow);
        }

        private IEnumerable<TiberiumCrystal> TiberiumCrystalsInRegion(Region region)
        {
            return region.ListerThings.AllThings.OfType<TiberiumCrystal>();
        }

        public bool HarvestableTiberiumCrystals(Region region, Harvester harvester, out IEnumerable<TiberiumCrystal> harvestableCrystals)
        {
            harvestableCrystals = TiberiumCrystalsInRegion(region).Where(c => c.HarvestableNow && c.CanBeHarvestedBy(harvester));
            return harvestableCrystals.Any();
        } 

        public TiberiumCrystal FindClosestTiberiumFor(Harvester harvester)
        {
            Map map = harvester.Map;
            IntVec3 rootPos = harvester.Position;
            Region rootRegion = rootPos.GetRegion(map, RegionType.Set_Passable);
            if (rootRegion == null) return null;

            TraverseParms parms = TraverseParms.For(harvester, Danger.Deadly, TraverseMode.ByPawn);
            bool EntryCondition(Region fromRegion, Region to) => to.Allows(parms, false);

            float currentClosest = 9999999f;
            TiberiumCrystal crystal = null;
            bool Processor(Region region)
            {
                if ((!region.IsDoorway && !region.Allows(parms, true)) || region.IsForbiddenEntirely(parms.pawn)) return false;
                if (!HarvestableTiberiumCrystals(region, harvester, out IEnumerable<TiberiumCrystal> crystalList)) return false;

                foreach (var crystal2 in crystalList)
                {
                    if (!ReachabilityWithinRegion.ThingFromRegionListerReachable(crystal2, region, PathEndMode.Touch, harvester)) continue;
                    float distance = (float) (crystal2.Position - rootPos).LengthHorizontalSquared;
                    if(distance < currentClosest)
                    {
                        crystal = crystal2;
                        currentClosest = distance;
                    }
                }
                return crystal != null;
            }
            RegionTraverser.BreadthFirstTraverse(rootPos, map, EntryCondition, Processor);
            return crystal;
        }

        /*
        public List<TiberiumCrystal> FindClosestTiberiumGroupFor(Harvester harvester)
        {
            IntVec3 rootPos = harvester.Position;
            Map map = harvester.Map;
            TraverseParms parms = TraverseParms.For(harvester, Danger.Some, TraverseMode.ByPawn);
            RegionEntryPredicate entryCondition = (Region from, Region to) => to.Allows(parms, false);

            List<TiberiumCrystal> crystals = null;
            int valueLeft = (int)(harvester.Container.capacity - harvester.Container.TotalStored);
            RegionProcessor processor = delegate (Region region)
            {
                if ((!region.IsDoorway && !region.Allows(parms, true)) || region.IsForbiddenEntirely(parms.pawn))
                    return false;
                if (tiberiumRegions.TryGetValue(region, out TiberiumRegion tibRegion))
                {
                    foreach (var crystal in tibRegion.CrystalList)
                    {

                    }
                }
                return valueLeft <= 0;
            };
            RegionTraverser.BreadthFirstTraverse(rootPos, map, entryCondition, processor);
            return crystals;
        }
        */
    }
}