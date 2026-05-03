using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace TiberiumRim;

public class HarvesterReservationManager
{
    private int Current;
    public KeyValuePair<TR.Harvester, TiberiumCrystal> CurrentPair;
    private bool Finished = true;
    public Map map;
    public Dictionary<TR.Harvester, TiberiumCrystal> Reservations = new();
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
    private MapComponent_TNWManager TNWManager => map.GetComponent<MapComponent_TNWManager>();

    private TR.Harvester CurHarvester => CurrentPair.Key;
    private bool CurPairValid => TargetValidFor(CurrentPair.Key);
    public bool NeedsUpdate => Reservations.Keys.Any(k => !TargetValidFor(k));

    public void Setup()
    {
        ReservedTypes.Add(HarvestType.Valuable, 0);
        ReservedTypes.Add(HarvestType.Unvaluable, 0);
    }

    public bool TargetValidFor(TR.Harvester harvester)
    {
        if (Reservations.TryGetValue(harvester, out var value))
            return !value.DestroyedOrNull() && value.Spawned && value.CanBeHarvestedBy(harvester);
        return false;
    }

    public void RegisterHarvester(TR.Harvester harvester)
    {
        if (!Reservations.Keys.Contains(harvester)) Reservations.Add(harvester, null);
    }

    public void DeregisterHarvester(TR.Harvester harvester)
    {
        if (Reservations.Keys.Contains(harvester)) Reservations.Remove(harvester);
    }

    private void Reserve(TiberiumCrystal tib, TR.Harvester harvester)
    {
        Reservations[harvester] = tib;
        ReservedTypes[tib.def.HarvestType]++;
        ReservedTotal++;
    }

    public void UnreserveFor(TiberiumCrystal tib, TR.Harvester harvester)
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

        var potentialPasses = Reservations.Keys.Count -
                              Mathf.Clamp(Reservations.Keys.Count - TiberiumManager.TiberiumInfo.TotalCount, 0,
                                  int.MaxValue);
        Finished = false;
        Current = 0;
        Predicate<IntVec3> passCheck = x => x.IsValid && x.Standable(map) && !Finished;
        var processor = delegate(IntVec3 c)
        {
            RETRY:
            if (Current < potentialPasses)
            {
                CurrentPair = Reservations.ElementAt(Current);

                if (CurPairValid || !CurHarvester.ShouldHarvest)
                {
                    Current++;
                    goto RETRY;
                }

                TiberiumCrystal crystal = c.TryGetTiberiumFor(CurHarvester);
                if (crystal != null && CurHarvester.CanReserve(crystal) && CurHarvester.CanReach(c, PathEndMode.Touch,
                        Danger.Deadly, false, TraverseMode.PassDoors))
                {
                    Reserve(crystal, CurHarvester);
                    Current++;
                }
            }
            else
            {
                Finished = true;
            }
        };
        map.floodFiller.FloodFill(Reservations.First().Key.Position, passCheck, processor, int.MaxValue, true,
            Reservations.Keys.Select(h => h.Position));
        foreach (var harvi in Reservations.Keys)
            if (Reservations[harvi] == null)
                harvi.SetToWait();
    }
}