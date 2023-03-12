using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace TiberiumRim
{
    public class JobGiver_HarvestTiberium : ThinkNode_JobGiver
    {
        public override Job TryGiveJob(Pawn pawn)
        {
            Harvester harvester = pawn as Harvester;

            if (harvester.CurrentPriority != HarvesterPriority.Harvest) return null;
            if (harvester.IsHarvesting) return null;

            TiberiumCrystal crystal = pawn.Map.Tiberium().HarvesterInfo.FindClosestTiberiumFor(harvester);
            if (crystal == null)
            {
                harvester.Notify_CouldNotFindTib();
                return null;
            }

            Job job = JobMaker.MakeJob(TiberiumDefOf.HarvestTiberium, crystal);
           
            // job.targetQueueA = new List<LocalTargetInfo>();
            // foreach (var t in queue)
            // {
            //     job.targetQueueA.Add(t);
            // }
            return job;
        }
    }

    public class JobDriver_HarvestTiberium : JobDriver
    {
        private int ticksPerValue = 0;
        private float growthPerValue = 0;
        private int ticksToHarvest = 0;
        private int ticksPassed = 0;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref ticksPerValue, "ticksPerValue");
            Scribe_Values.Look(ref growthPerValue, "growthPerValue");
            Scribe_Values.Look(ref ticksToHarvest, "ticksToHarvest");
            Scribe_Values.Look(ref ticksPassed, "ticksPassed");
        }

        private TiberiumCrystal TiberiumCrystal => TargetA.Thing as TiberiumCrystal;

        private Harvester Harvester => pawn as Harvester;

        private bool FailOn => Harvester.PlayerInterrupt || !Harvester.CanHarvestTiberium(TiberiumCrystal.def);

        public override string GetReport()
        {
            return "TR_HarvestingReport".Translate(this.TargetA.Thing.def.LabelCap);
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            LocalTargetInfo target = job.GetTarget(TargetIndex.A);
            if (pawn.Reserve(target, job, 1, -1, null, errorOnFailed))
            {
                pawn.ReserveAsManyAsPossible(job.GetTargetQueue(TargetIndex.A), job);
                return true;
            }
            return false;
        }

        public override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_JobTransforms.MoveCurrentTargetIntoQueue(TargetIndex.A);
            var extractTarget = Toils_JobTransforms.ClearDespawnedNullOrForbiddenQueuedTargets(TargetIndex.A);
            yield return extractTarget;
            yield return Toils_JobTransforms.SucceedOnNoTargetInQueue(TargetIndex.A);
            yield return Toils_JobTransforms.ExtractNextTargetFromQueue(TargetIndex.A);

            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch).JumpIfDespawnedOrNullOrForbidden(TargetIndex.A, extractTarget);

            Toil harvest = new Toil
            {
                initAction = delegate
                {
                    //Time based on each weight per Tick 
                    ticksToHarvest =  (int)Math.Round((TiberiumCrystal.HarvestValue / Harvester.kindDef.harvestValue), MidpointRounding.AwayFromZero);
                    //Ticks Needed to get 1 single weight stored
                    ticksPerValue = (int) (ticksToHarvest / TiberiumCrystal.HarvestValue);
                    //Growth removed whenever weight is added
                    growthPerValue = (TiberiumCrystal.Growth / (float) ticksToHarvest) * ticksPerValue;
                },
                tickAction = delegate
                {
                    if (Harvester.Container.Full)
                    {
                        EndJobWith(JobCondition.InterruptForced);
                        return;
                    }

                    if (ticksPassed > ticksToHarvest)
                    {
                        if (TiberiumCrystal.Spawned && !Harvester.Container.Full)
                        {
                            TiberiumCrystal.DeSpawn();
                        }
                        ticksPassed = 0;
                        ReadyForNextToil();
                        return;
                    }

                    if (ticksPassed % ticksPerValue == 0)
                    {
                        Harvester.Animator.Start("Harvest", true);
                        TiberiumCrystal.Harvest(Harvester, growthPerValue);
                    }
                    ticksPassed++;
                }
            };
            //harvest.AddFinishAction(() => Harvester.TNWManager.ReservationManager.Dequeue(TiberiumCrystal, Harvester));
            harvest.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            harvest.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
            harvest.FailOn(() => FailOn);
            harvest.defaultCompleteMode = ToilCompleteMode.Never;
            harvest.AddFinishAction(Harvester.Animator.Stop);
            yield return harvest;
            yield return Toils_Jump.Jump(extractTarget);
        }
    }
}
