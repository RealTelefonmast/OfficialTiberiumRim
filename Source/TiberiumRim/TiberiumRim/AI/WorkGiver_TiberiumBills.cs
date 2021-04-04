using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using Verse.AI;

namespace TiberiumRim
{
    public class WorkGiver_TiberiumBills : WorkGiver_Scanner
    {
		public override PathEndMode PathEndMode => PathEndMode.InteractionCell;

        public override ThingRequest PotentialWorkThingRequest
        {
            get
            {
                return ThingRequest.ForGroup(ThingRequestGroup.Nothing);
            }
        }

        public IEnumerable<Thing> Targets(Map map)
        {
            var tnw = map.Tiberium().TNWManager;
            var buildings = tnw.MainStructureSet.Crafters.Select(t => t.parent);
            return buildings;
        }

        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            return Targets(pawn.Map);
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if(t is ThingWithComps thing && !thing.IsPowered(out _)) return false;

            var compTNW = t.TryGetComp<CompTNW_Crafter>();
            if (compTNW == null) return false;
            if (!compTNW.Network.IsWorking) return false;
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
