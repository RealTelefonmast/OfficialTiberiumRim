using System.Collections.Generic;
using System.Linq;
using TeleCore.Network;
using UnityEngine;
using Verse;
using Verse.AI;

namespace TiberiumRim
{
    public class HarvesterReservationManager
    {
        public Map map;
        public List<Harvester> AllHarvesters = new List<Harvester>();
        public Dictionary<Harvester, List<TiberiumCrystal>> ReservedQueues = new Dictionary<Harvester, List<TiberiumCrystal>>();
        public Dictionary<HarvestType, int> ReservedTypes = new Dictionary<HarvestType, int>();

        public int ReservedTotal = 0;
        private int PossiblePasses = 0;
        private int Current = 0;
        private bool Finished = true;

        public HarvesterReservationManager()
        {
            Setup();
        }

        public HarvesterReservationManager(Map map)
        {
            this.map = map;
            Setup();
        }  

        public void Setup()
        {
            ReservedTypes.Add(HarvestType.Valuable, 0);
            ReservedTypes.Add(HarvestType.Unvaluable, 0);
        }

        private MapComponent_Tiberium TiberiumManager => map.Tiberium();
        private DynamicNetworkGraph NetworkMaster => TiberiumManager.NetworkInfo[TiberiumDefOf.TiberiumNetwork];

        private Harvester CurHarvester => AllHarvesters[Current];

        public bool IsQueued(TiberiumCrystal crystal)
        {
            return ReservedQueues.Values.Any(c => c.Contains(crystal));
        }

        public bool TargetValidFor(Harvester harvester)
        {
            ReservedQueues[harvester].RemoveAll(t => t == null);
            return !ReservedQueues[harvester].NullOrEmpty() && ReservedQueues[harvester].Any();
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
            var value = ReservedQueues[harvester].Sum(t => t.HarvestValue) + harvester.Container.TotalValue;
            return value >= harvester.Container.MaxCapacity;
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
            PossiblePasses = AllHarvesters.Count - Mathf.Clamp(AllHarvesters.Count - TiberiumManager.TiberiumInfo.TotalCount, 0, int.MaxValue);

            Finished = false;
            Current = 0;
            bool PassCheck(IntVec3 x) => x.IsValid && x.Walkable(map) && !Finished;
            void Processor(IntVec3 c)
            {
                RETRY:
                if (Current >= PossiblePasses)
                {
                    Finished = true;
                    return;
                }

                if (QueueFull(CurHarvester) || CurHarvester.CurrentPriority != HarvesterPriority.Harvest) { Current++; goto RETRY; }

                TiberiumCrystal crystal = c.TryGetTiberiumFor(CurHarvester);
                if (crystal != null && !IsQueued(crystal) && CurHarvester.CanReach(c, PathEndMode.Touch, Danger.Deadly, false, false,TraverseMode.PassDoors))
                {
                    Enqueue(crystal, CurHarvester);
                }
            }
            
            map.floodFiller.FloodFill(CurHarvester.Position, PassCheck, Processor, int.MaxValue, true, AllHarvesters.Select(h => h.Position));
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
}
