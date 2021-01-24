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
            return Manager.currentProject == null || !Manager.currentProject.CurrentTask.HasAnyTarget;
        }

        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            if (CurrentTask?.HasSingleTarget ?? true) return null;
            return CurrentTask.TargetThings();;
        }

        public override bool Prioritized => true;

        private TResearchManager Manager => TRUtils.ResearchManager();
        private TResearchTaskDef CurrentTask => Manager.currentProject?.CurrentTask;

        //The pawn needs to be able to do the worktype
        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            TResearchDef currentProj = TRUtils.ResearchManager().currentProject;
            if (currentProj == null) return false;
            if (!(t as ThingWithComps).IsPowered(out bool usesPower) && usesPower) return false;
            if (!PawnCapable(pawn, out string reason))
            {
                JobFailReason.Is("\n" +  reason, null);
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
                reason += "TR_ResearchInactiveWorkType".Translate(CurrentTask.WorkType.labelShort) + "\n";
            }

            if (!CurrentTask.SkillRequirements.NullOrEmpty())
            {
                string missingSkills = "";
                foreach (var skillReq in CurrentTask.SkillRequirements)
                {
                    if (!skillReq.PawnSatisfies(pawn))
                        missingSkills += "  - " + skillReq.skill.skillLabel + " ("+ skillReq.minLevel + ")\n";
                }

                if (!missingSkills.NullOrEmpty())
                    reason += "TR_ResearchMissingSkill".Translate(missingSkills) + "\n";
            }

            return reason.TrimEndNewlines().NullOrEmpty();
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            return JobMaker.MakeJob(TiberiumDefOf.TiberiumResearch, t);
        }

        public override float GetPriority(Pawn pawn, TargetInfo t)
        {
            return t.Thing.GetStatValue(CurrentTask.RelevantPawnStat ?? StatDefOf.ResearchSpeedFactor, true);
        }
    }
}
