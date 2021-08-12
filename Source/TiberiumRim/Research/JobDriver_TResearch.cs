using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace TiberiumRim
{
    public class JobDriver_TResearch : JobDriver
    {
        [Unsaved]
        private IntVec3 targetPos = IntVec3.Invalid;


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

        private PathEndMode PathEndMode
        {
            get
            {
                if (ResearchThing.def.hasInteractionCell)
                    return PathEndMode.InteractionCell;
                return PathEndMode.OnCell;
            }
        }

        private IntVec3 TargetPos
        {
            get
            {
                if (targetPos.IsValid) return targetPos;
                var distance = Project.CurrentTask.distanceFromTarget;
                var atTarget  = distance <= 0;

                if (atTarget)
                {
                    targetPos = TargetA.Thing.RandomAdjacentCell8Way();
                }
                else
                {
                    var width = Project.CurrentTask.distanceRange;
                    var minDistance = distance - width / 2;
                    var maxDistance = distance + width / 2;
                    bool Predicate(IntVec3 x)
                    {
                        if (!x.Standable(Map)) return false;
                        float distanceTo = TargetA.Cell.DistanceTo(x);
                        if (distanceTo > maxDistance || distanceTo < minDistance) return false;
                        return GenSight.LineOfSight(TargetA.Cell, x, Map);
                    }
                    CellFinder.TryFindRandomCellNear(TargetA.Cell, Map, Mathf.CeilToInt(distance), Predicate, out targetPos);
                    //targetPos = CellFinder.TryFindRandomReachableCellNear(TargetA.Cell, Map, Mathf.CeilToInt(distance), TraverseParms.For(TraverseMode.ByPawn, Danger.Some,false), Predicate));
                }
                return targetPos;
            }
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            yield return Toils_Reserve.Reserve(TargetIndex.A, 1, -1, null);
            yield return PathEndMode == PathEndMode.InteractionCell ? Toils_Goto.GotoCell(TargetIndex.A, PathEndMode) : Toils_Goto.GotoCell(TargetPos, PathEndMode);

            TResearchTaskDef task = Project.CurrentTask;
            Toil research = new Toil();
            research.tickAction = delegate
            {
                Pawn pawn = research.actor;
                float num = 1;
                if (task.RelevantPawnStat != null) 
                    num *= pawn.GetStatValue(task.RelevantPawnStat, true);
                if (task.RelevantTargetStat != null)
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
