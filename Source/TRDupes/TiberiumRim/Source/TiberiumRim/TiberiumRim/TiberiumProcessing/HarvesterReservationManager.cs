using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using Verse.AI;
using UnityEngine;

namespace TiberiumRim
{
    public class HarvesterReservationManager
    {
        public Map map;
        public Dictionary<Harvester, TiberiumCrystal> Reservations = new Dictionary<Harvester, TiberiumCrystal>();
        public Dictionary<HarvestType, int> ReservedTypes = new Dictionary<HarvestType, int>();
        public KeyValuePair<Harvester, TiberiumCrystal> CurrentPair;
        public int ReservedTotal = 0;
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

        private MapComponent_Tiberium TiberiumManager => map.GetComponent<MapComponent_Tiberium>();
        private MapComponent_TNWManager TNWManager => map.GetComponent<MapComponent_TNWManager>();

        private Harvester CurHarvester => CurrentPair.Key;
        private bool CurPairValid => TargetValidFor(CurrentPair.Key);
        public bool NeedsUpdate => Reservations.Keys.Any(k => !TargetValidFor(k));

        public bool TargetValidFor(Harvester harvester)
        {           
            if (Reservations.TryGetValue(harvester, out TiberiumCrystal value))
            {
                return !value.DestroyedOrNull() && value.Spawned && value.CanBeHarvestedBy(harvester);
            }
            return false;
        }

        public void RegisterHarvester(Harvester harvester)
        {
            if (!Reservations.Keys.Contains(harvester))
            {
                Reservations.Add(harvester, null);
            }
        }

        public void DeregisterHarvester(Harvester harvester)
        {
            if (Reservations.Keys.Contains(harvester))
            {
                Reservations.Remove(harvester);
            }
        }

        private void Reserve(TiberiumCrystal tib, Harvester harvester)
        {
            Reservations[harvester] = tib;
            ReservedTypes[tib.def.HarvestType]++;
            ReservedTotal++;
        }

        public void UnreserveFor(TiberiumCrystal tib, Harvester harvester)
        {
            if (tib != null)
            {
                Reservations[harvester] = null;
                ReservedTypes[tib.def.HarvestType]--;
                ReservedTotal--;
            }
        }

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
    }
}
