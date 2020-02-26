using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace TiberiumRim
{
    public class JobDriver_TResearch : JobDriver
    {
        private TResearchManager Manager => TRUtils.ResearchManager();

        private TResearchDef Project => Manager.currentProject;

        private Thing ResearchThing => base.TargetThingA;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job, 1, -1, null, true);
        }

        private bool HasEffect => Effect != null;

        private EffecterDef Effect
        {
            get
            {
                var workType = Project.CurrentTask.WorkType;
                if (workType.Equals(WorkTypeDefOf.Crafting))
                    return EffecterDefOf.ConstructMetal;
                return EffecterDefOf.Research;
            }
        }

        private PathEndMode PathMode
        {
            get
            {
                if (ResearchThing.def.hasInteractionCell)
                    return PathEndMode.InteractionCell;
                return PathEndMode.OnCell;
            }
        }

        private IntVec3 TargetPosition
        {
            get
            {
                var distance = Project.CurrentTask.distanceFromTarget;
                var atTarget = distance <= 0;
                if (atTarget)
                {
                    if (TargetA.Thing.def.hasInteractionCell)
                        return TargetA.Thing.InteractionCell;
                    return TargetA.Thing.RandomAdjacentCell8Way();
                }

                bool Predicate(IntVec3 x)
                {
                    float dist = TargetA.Cell.DistanceTo(x);
                    return dist >= distance && dist <= distance;
                }

                return CellFinder.RandomClosewalkCellNear(TargetA.Cell, Map, Mathf.CeilToInt(distance), Predicate);
            }
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            yield return Toils_Reserve.Reserve(TargetIndex.A, 1, -1, null);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);

            TResearchTaskDef task = Project.CurrentTask;
            Toil research = new Toil();
            research.tickAction = delegate
            {
                Pawn pawn = research.actor;
                float num = pawn.GetStatValue(task.RelevantPawnStat, true);
                num *= TargetThingA.GetStatValue(task.RelevantTargetStat, true);
                Manager.PerformResearch(task, pawn, num);
                if (!task.SkillRequirements.NullOrEmpty())
                {
                    foreach (var skillReq in task.SkillRequirements)
                    {
                        pawn.skills.GetSkill(skillReq.skill).Learn(0.11f, false);
                    }
                }
                pawn.GainComfortFromCellIfPossible();
            };
            research.FailOnCannotTouch(TargetIndex.A, PathMode);
            research.FailOn(() => Project == null || task.WorkType != Project.CurrentTask.WorkType);
            if(HasEffect)
                research.WithEffect(Effect, TargetIndex.A);
            research.WithProgressBar(TargetIndex.A, () => task.ProgressPct);
            research.defaultCompleteMode = ToilCompleteMode.Delay;
            research.defaultDuration = 4000;
            yield return research;
            yield return Toils_General.Wait(2, TargetIndex.None);
        }
    }
}
