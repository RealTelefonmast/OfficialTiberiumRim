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
        public override ThingRequest PotentialWorkThingRequest
        {
            get
            {
                if (Manager.currentProject != null && CurrentTask.HasSingleTarget)
                    return ThingRequest.ForDef(CurrentTask.MainTarget);
                return ThingRequest.ForGroup(ThingRequestGroup.Nothing);
            }
        }

        public override bool ShouldSkip(Pawn pawn, bool forced = false)
        {
            return Manager.currentProject == null;
        }

        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            if (CurrentTask?.HasSingleTarget ?? true) return null;
            return ResearchTargetTable.GetTargetsFor(Manager.currentProject.CurrentTask);
        }

        private TResearchManager Manager => TRUtils.ResearchManager();
        private TResearchTaskDef CurrentTask => Manager.currentProject?.CurrentTask;

        //The pawn needs to be able to do the worktype
        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            TResearchDef currentProj = TRUtils.ResearchManager().currentProject;
            if (currentProj == null) return false;

            if (!PawnCapable(pawn, out string reason))
            {
                JobFailReason.Is("TR_ResearchPawnIncapable".Translate(reason), null);
                return false;
            }
            return pawn.CanReserve(t, 1, -1, null, forced);
            
        }

        private bool PawnCapable(Pawn pawn, out string reason)
        {
            reason = "";
            bool canDoWork = pawn.workSettings.WorkIsActive(CurrentTask.WorkType);
            if (!canDoWork)
            {
                reason += "TR_ResearchMissingWorkType".Translate(CurrentTask.WorkType) + "\n";
            }

            if (!CurrentTask.SkillRequirements.NullOrEmpty())
            {
                foreach (var skillReq in CurrentTask.SkillRequirements)
                {
                    if (!skillReq.PawnSatisfies(pawn))
                        reason += "TR_ResearchMissingSkill".Translate(skillReq.skill, skillReq.minLevel) + "\n";
                }
            }

            return reason.TrimEndNewlines().NullOrEmpty();
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            return JobMaker.MakeJob(TiberiumDefOf.TiberiumResearch, t);
        }

        public override float GetPriority(Pawn pawn, TargetInfo t)
        {
            return t.Thing.GetStatValue(CurrentTask.RelevantPawnStat, true);
        }
    }
}
