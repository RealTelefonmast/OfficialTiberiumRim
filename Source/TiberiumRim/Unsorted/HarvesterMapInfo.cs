using System.Collections.Generic;
using System.Linq;
using RimWorld;
using TiberiumRim;
using Verse;
using Verse.AI;
using MapComponent_Tiberium = TR.Components.MapComponent_Tiberium;

namespace TR.Info;

public class HarvesterMapInfo : MapInformation
{
    public List<Harvester> AllHarvesters = new();

    public HarvesterMapInfo(Map map) : base(map)
    {
        //harvestableBools = new BoolGrid(map);
    }

    private MapComponent_Tiberium Tiberium => map.Tiberium();

    private TiberiumGrid TiberiumGrid => Tiberium.TiberiumInfo.TiberiumGrid;

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

    public bool HarvestableTiberiumCrystals(Region region, Harvester harvester,
        out IEnumerable<TiberiumCrystal> harvestableCrystals)
    {
        harvestableCrystals = TiberiumCrystalsInRegion(region)
            .Where(c => c.HarvestableNow && c.CanBeHarvestedBy(harvester));
        return harvestableCrystals.Any();
    }

    public TiberiumCrystal FindClosestTiberiumFor(Harvester harvester)
    {
        var rootPos = harvester.Position;
        var map = harvester.Map;
        var parms = TraverseParms.For(harvester);

        var rootRegion = rootPos.GetRegion(map);
        if (rootRegion == null)
            return null;

        bool EntryCondition(Region fromRegion, Region to)
        {
            return to.Allows(parms, false);
        }

        TiberiumCrystal crystal = null;

        var currentClosest = 9999999f;

        bool Processor(Region region)
        {
            if ((!region.IsDoorway && !region.Allows(parms, true)) || region.IsForbiddenEntirely(parms.pawn))
                return false;
            if (!HarvestableTiberiumCrystals(region, harvester, out var crystalList)) return false;

            foreach (var crystal2 in crystalList)
            {
                if (!ReachabilityWithinRegion.ThingFromRegionListerReachable(crystal2, region, PathEndMode.Touch,
                        harvester)) continue;
                var distance = (float)(crystal2.Position - rootPos).LengthHorizontalSquared;
                if (distance < currentClosest)
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
        int valueLeft = (int)(harvester.Container.capacity - harvester.Container.TotalStorage);
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