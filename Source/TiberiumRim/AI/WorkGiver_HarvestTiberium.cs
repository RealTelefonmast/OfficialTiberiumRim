using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace TiberiumRim
{
    public class WorkGiver_HarvestTiberium : WorkGiver_Scanner
    {
        public override PathEndMode PathEndMode => PathEndMode.ClosestTouch;
        public override bool Prioritized => true;
        public override bool AllowUnreachable => false;


        public override Danger MaxPathDanger(Pawn pawn)
        {
            return Danger.Deadly;
        }

        public override bool ShouldSkip(Pawn pawn, bool forced = false)
        {
            if (pawn is Harvester harvester)
            {
                return harvester.HarvestMode == HarvestMode.Moss ? harvester.Map.Tiberium().MossAvailable : harvester.Map.Tiberium().TiberiumAvailable;
            }
            return true;
        }

        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            if (pawn is Harvester harvester)
            {
                if (harvester.Container.CapacityFull) return null;
                var manager = pawn.Map.GetComponent<MapComponent_Tiberium>();
                return manager.TiberiumInfo.AllTiberiumCrystals;
            }
            return null;
        }

        public override IEnumerable<IntVec3> PotentialWorkCellsGlobal(Pawn pawn)
        {
            return base.PotentialWorkCellsGlobal(pawn);
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            var harvester = pawn as Harvester;
            var crystal = t as TiberiumCrystal;

            if (!pawn.CanReserve(t, 1, -1, null, forced))
            {
                return null;
            }

            if ((crystal?.CanBeHarvestedBy(harvester) ?? false) && harvester.CanReserveAndReach(crystal, PathEndMode.ClosestTouch, Danger.Deadly))
                return JobMaker.MakeJob(TiberiumDefOf.HarvestTiberium, crystal);
            return null;
        }
    }
}
