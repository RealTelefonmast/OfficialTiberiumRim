using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;

namespace TiberiumRim
{
    public class WorkGiver_TResearch : WorkGiver_Scanner
    {
        public override ThingRequest PotentialWorkThingRequest =>
            ThingRequest.ForDef(TRUtils.ResearchManager().currentProj.researchThing);

        public override bool Prioritized => true;

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            Thing researchThing = t;
            TResearchDef currentProj = TRUtils.ResearchManager().currentProj;
            if (currentProj == null) return false;
            //TODO: Add Missing Checks
            return base.HasJobOnThing(pawn, t, forced);
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            return new Job(TiberiumDefOf.TiberiumResearch, t);
        }

        public override float GetPriority(Pawn pawn, TargetInfo t)
        {
            return t.Thing.GetStatValue(StatDefOf.ResearchSpeedFactor, true);
        }
    }
}
