using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace TiberiumRim
{
    public class WorkGiver_NetworkBills : WorkGiver_Scanner
    {
		public override PathEndMode PathEndMode => PathEndMode.InteractionCell;
        public override Danger MaxPathDanger(Pawn pawn) => Danger.Some;

        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForUndefined();

        public IEnumerable<Thing> Targets(Map map)
        {
            return map.Tiberium().StructureCacheInfo.GetThingsFromGroup(TRGroupDefOf.TiberiumCrafters);
        }

        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            return Targets(pawn.Map);
        }

        public override bool ShouldSkip(Pawn pawn, bool forced = false)
        {
            return false;
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if(t is ThingWithComps thing && !thing.IsPoweredOn()) return false;

            var compTNW = t.TryGetComp<Comp_NetworkStructureCrafter>();
            if (compTNW == null) return false;
            if (compTNW.BillStack.Count == 0) return false;
            if (compTNW.BillStack.ParentNetComps.Any(t => !t.Network.IsWorking)) return false;
            if (compTNW.billStack.CurrentBill != null)
            {
                if (!compTNW.billStack.CurrentBill.ShouldDoNow()) return false;
                return !t.IsReserved(out _);
            }
            return false;
        }

        public override Job JobOnThing(Pawn pawn, Thing thing, bool forced = false)
        {
            return new Job(TiberiumDefOf.TiberiumBill, thing);
        }
	}
}
