using System.Collections.Generic;
using System.Linq;
using RimWorld;
using TeleCore.Static;
using TR.Buildings;
using TR.DefOf;
using UnityEngine;
using Verse;
using Verse.AI;

namespace TR.Data.ThingClasses;

public class WorkGiver_DoMechConstructionBill : WorkGiver_Scanner
{
    private static readonly List<ThingDefCountClass> missingResources = new();
    private static readonly List<Thing> resourcesAvailable = new();

    public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
    {
        return pawn.Map.ThingGroupCache().ThingsOfGroup(TRCDefOf.MechHangars);
    }

    public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
    {
        if (t.Faction != pawn.Faction) return null;

        //Start hauling items to the hangar
        if (t is Building_Hangar hangar)
            if (hangar.Bills is { HasBillWaiting: true })
                return ResourceDeliverJobFor(pawn, hangar);

        return null;
    }

    private static Job ResourceDeliverJobFor(Pawn pawn, Building_Hangar hangar)
    {
        missingResources.Clear();
        foreach (var need in hangar.Bills.CurBill.MissingResources)
            if (!pawn.Map.itemAvailability.ThingsAvailableAnywhere(need.thingDef, need.count, pawn))
            {
                missingResources.Add(need);
                if (FloatMenuMakerMap.makingFor != pawn) break;
            }
            else
            {
                var foundRes = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map,
                    ThingRequest.ForDef(need.thingDef), PathEndMode.ClosestTouch,
                    TraverseParms.For(pawn), 9999f,
                    r => WorkGiver_ConstructDeliverResources.ResourceValidator(pawn, need, r));

                if (foundRes == null)
                {
                    missingResources.Add(need);
                    if (FloatMenuMakerMap.makingFor != pawn) break;
                    continue;
                }

                FindAvailableNearbyResources(foundRes, pawn, out var resTotalAvailable);

                var job = new Job(TRCDefOf.DoMechConstructionBill);
                job.targetA = hangar;
                job.targetB = foundRes;

                job.targetQueueB = new List<LocalTargetInfo>();
                var available = 0;
                for (var i = 0; i < resourcesAvailable.Count; i++)
                    if (available < need.count)
                    {
                        job.targetQueueB.Add(resourcesAvailable[i]);
                        available += resourcesAvailable[i].stackCount;
                    }
                    else
                    {
                        break;
                    }

                job.count = Mathf.Min(need.count, available);
                job.haulMode = HaulMode.ToContainer;
                return job;
            }

        if (missingResources.Count > 0 && FloatMenuMakerMap.makingFor == pawn)
            JobFailReason.Is("MissingMaterials".Translate(
                (from need in missingResources
                    select need.Summary).ToCommaList()));

        return null;
    }

    private static void FindAvailableNearbyResources(Thing firstFoundResource, Pawn pawn, out int resTotalAvailable)
    {
        var num = Mathf.Min(firstFoundResource.def.stackLimit,
            pawn.carryTracker.MaxStackSpaceEver(firstFoundResource.def));
        resTotalAvailable = 0;
        resourcesAvailable.Clear();
        resourcesAvailable.Add(firstFoundResource);
        resTotalAvailable += firstFoundResource.stackCount;
        if (resTotalAvailable >= num) return;
        foreach (var thing in GenRadial.RadialDistinctThingsAround(firstFoundResource.Position, firstFoundResource.Map,
                     5f, false))
        {
            if (resTotalAvailable >= num) break;
            if (thing.def == firstFoundResource.def && GenAI.CanUseItemForWork(pawn, thing))
            {
                resourcesAvailable.Add(thing);
                resTotalAvailable += thing.stackCount;
            }
        }
    }
}