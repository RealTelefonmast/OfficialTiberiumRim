using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using  UnityEngine;
using Verse;
using  RimWorld;
using Verse.AI;
using Verse.AI.Group;

namespace TiberiumRim
{
    public class WorkGiver_HarvestTiberium : WorkGiver_Scanner
    {
        public override PathEndMode PathEndMode => PathEndMode.ClosestTouch;
        public override bool Prioritized => true;
        public override bool AllowUnreachable => false;

        public override bool ShouldSkip(Pawn pawn, bool forced = false)
        {
            return pawn.GetLord() != null || base.ShouldSkip(pawn, forced);
        }

        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            if (pawn is Harvester harvester && harvester.Container.CapacityFull) return null;

            var manager = pawn.Map.GetComponent<MapComponent_Tiberium>();
            return manager.TiberiumInfo.AllTiberiumCrystals;
        }

        public override IEnumerable<IntVec3> PotentialWorkCellsGlobal(Pawn pawn)
        {
            return base.PotentialWorkCellsGlobal(pawn);
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            var harvester = pawn as Harvester;
            var crystal = t as TiberiumCrystal;

            if((crystal?.CanBeHarvestedBy(harvester) ?? false) && harvester.CanReserveAndReach(crystal, PathEndMode.ClosestTouch, Danger.Deadly))
                return JobMaker.MakeJob(TiberiumDefOf.HarvestTiberium, crystal);
            return null;
        }
    }
}
