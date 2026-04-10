using System.Collections.Generic;
using System.Linq;
using TR.Components;
using UnityEngine;
using Verse;
using Verse.AI;

namespace TR;

public class HarvesterReservationManager
{
    public List<Harvester> AllHarvesters = new();
    private int Current;
    private bool Finished = true;
    public Map map;
    private int PossiblePasses;
    public Dictionary<Harvester, List<TiberiumCrystal>> ReservedQueues = new();

    public int ReservedTotal;
    public Dictionary<HarvestType, int> ReservedTypes = new();

    public HarvesterReservationManager()
    {
        Setup();
    }

    public HarvesterReservationManager(Map map)
    {
        this.map = map;
        Setup();
    }

    private MapComponent_Tiberium TiberiumManager => map.GetComponent<MapComponent_Tiberium>();

    private Harvester CurHarvester => AllHarvesters[Current];

    public void Setup()
    {
        ReservedTypes.Add(HarvestType.Valuable, 0);
        ReservedTypes.Add(HarvestType.Unvaluable, 0);
    }

    public bool IsQueued(TiberiumCrystal crystal)
    {
        return ReservedQueues.Values.Any(c => c.Contains(crystal));
    }

    public bool TargetValidFor(Harvester harvester)
    {
        ReservedQueues[harvester].RemoveAll(t => t == null);
        return !ReservedQueues[harvester].NullOrEmpty() && Enumerable.Any(ReservedQueues[harvester]);
    }

    public void RegisterHarvester(Harvester harvester)
    {
        if (!AllHarvesters.Contains(harvester))
        {
            AllHarvesters.Add(harvester);
            ReservedQueues.Add(harvester, new List<TiberiumCrystal>());
        }
    }

    public void DeregisterHarvester(Harvester harvester)
    {
        if (AllHarvesters.Contains(harvester))
        {
            AllHarvesters.Remove(harvester);
            ReservedQueues.Remove(harvester);
        }
    }

    private bool QueueFull(Harvester harvester)
    {
        var value = ReservedQueues[harvester].Sum(t => t.HarvestValue) + harvester.Container.TotalStorage;
        return value >= harvester.Container.capacity;
    }

    private void Enqueue(TiberiumCrystal tib, Harvester harvester)
    {
        ReservedQueues[harvester].Add(tib);
        ReservedTypes[tib.def.HarvestType]++;
        ReservedTotal++;
    }

    public void Dequeue(TiberiumCrystal tib, Harvester harvester)
    {
        if (tib == null) return;
        if (!ReservedQueues[harvester].NullOrEmpty())
            ReservedQueues[harvester].Remove(tib);

        ReservedTypes[tib.def.HarvestType]--;
        ReservedTotal--;
    }

    public void FillQueuesForExistingHarvesters()
    {
        PossiblePasses = AllHarvesters.Count -
                         Mathf.Clamp(AllHarvesters.Count - TiberiumManager.TiberiumInfo.TotalCount, 0, int.MaxValue);

        Finished = false;
        Current = 0;

        bool PassCheck(IntVec3 x)
        {
            return x.IsValid && map.pathing.normal.pathGrid.Walkable(x) && !Finished;
        }

        void Processor(IntVec3 c)
        {
            RETRY:
            if (Current >= PossiblePasses)
            {
                Finished = true;
                return;
            }

            if (QueueFull(CurHarvester) || CurHarvester.CurrentPriority != HarvesterPriority.Harvest)
            {
                Current++;
                goto RETRY;
            }

            var crystal = c.TryGetTiberiumFor(CurHarvester);
            if (crystal != null && !IsQueued(crystal) && CurHarvester.CanReach(c, PathEndMode.Touch, Danger.Deadly, false, TraverseMode.PassDoors))
                Enqueue(crystal, CurHarvester);
        }

        map.floodFiller.FloodFill(CurHarvester.Position, PassCheck, Processor, int.MaxValue, true,
            AllHarvesters.Select(h => h.Position));
    }

    /*
    public void TryUpdate()
    {
        if (!NeedsUpdate) return;

        int potentialPasses = Reservations.Keys.Count - Mathf.Clamp(Reservations.Keys.Count - TiberiumManager.TiberiumInfo.TotalCount, 0, int.MaxValue);
        Finished = false;
        Current = 0;
        Predicate<IntVec3> passCheck = x => x.IsValid && x.Standable(map) && !Finished;
        Action<IntVec3> processor = delegate (IntVec3 c)
        {
            RETRY:
            if (Current < potentialPasses)
            {
                CurrentPair = Reservations.ElementAt(Current);

                if (CurPairValid || !CurHarvester.ShouldHarvest) { Current++; goto RETRY; }
                TiberiumCrystal crystal = c.TryGetTiberiumFor(CurHarvester);
                if (crystal != null && CurHarvester.CanReserve(crystal) && CurHarvester.CanReach(c, PathEndMode.Touch, Danger.Deadly, false, TraverseMode.PassDoors))
                {
                    Reserve(crystal, CurHarvester);
                    Current++;
                }
            }
            else { Finished = true; }
        };
        map.floodFiller.FloodFill(Reservations.First().Key.Position, passCheck, processor, int.MaxValue, true, Reservations.Keys.Select(h => h.Position));
        foreach (var harvi in Reservations.Keys)
        {
            if (Reservations[harvi] == null)
                harvi.SetToWait();
        }
    }
    */
}