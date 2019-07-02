using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using  UnityEngine;
using Verse;
using  RimWorld;
using Verse.AI;

namespace TiberiumRim
{
    public class WorkGiver_HarvestTiberium : WorkGiver_Scanner
    {
        public override PathEndMode PathEndMode => PathEndMode.ClosestTouch;
        public override bool Prioritized => true;

        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            if (pawn is Harvester harvester)
            {
                var manager = pawn.Map.GetComponent<MapComponent_Tiberium>();
                return manager.TiberiumInfo.AllTiberiumCrystals;
            }
            return null;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            var harvester = pawn as Harvester;
            var crystal = t as TiberiumCrystal;

            if((crystal?.CanBeHarvestedBy(harvester) ?? false) && harvester.CanReserveAndReach(crystal, PathEndMode.ClosestTouch, Danger.Deadly))
                return new Job(TiberiumDefOf.HarvestTiberium, crystal);
            return null;
        }
    }
}
