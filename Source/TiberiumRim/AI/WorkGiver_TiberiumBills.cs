using System.Collections.Generic;
using RimWorld;
using TeleCore.ThingComps;
using Verse;
using Verse.AI;

namespace TR;

public class WorkGiver_TiberiumBills : WorkGiver_Scanner
{
    public override PathEndMode PathEndMode => PathEndMode.InteractionCell;

    public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForGroup(ThingRequestGroup.Nothing);

    public IEnumerable<Thing> Targets(Map map)
    {
        foreach (var building in map.listerBuildings.allBuildingsColonist)
            if (building.TryGetComp<Comp_NetworkBillsCrafter>() != null)
                yield return building;
    }

    public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
    {
        return Targets(pawn.Map);
    }

    public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
    {
        if (t is ThingWithComps thing && !thing.IsPowered(out _)) return false;

        var comp = t.TryGetComp<Comp_NetworkBillsCrafter>();
        if (comp == null) return false;
        if (comp.BillStack?.CurrentBill == null) return false;
        if (!comp.BillStack.CurrentBill.ShouldDoNow()) return false;
        return !t.IsReserved(out _);
    }

    public override Job JobOnThing(Pawn pawn, Thing thing, bool forced = false)
    {
        return new Job(TiberiumDefOf.TiberiumBill, thing);
    }
}
