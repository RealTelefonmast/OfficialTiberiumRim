﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;
using RimWorld;

namespace TiberiumRim
{
    public class JobGiver_HarvestTiberium : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            Harvester harvester = pawn as Harvester;
            if (harvester.ShouldHarvest && harvester.CurJob?.def != TiberiumDefOf.HarvestTiberium)
            {
                if (!harvester.HarvestQueue.NullOrEmpty())
                {
                    var queue = harvester.HarvestQueue;
                    Job job = JobMaker.MakeJob(TiberiumDefOf.HarvestTiberium, queue[0]);
                    job.targetQueueA = new List<LocalTargetInfo>();
                    foreach (var t in queue)
                    {
                        job.targetQueueA.Add(t);
                    }
                    return job;
                }
            }
            return null;
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

        private bool FailOn => Harvester.ShouldIdle || (TiberiumCrystal.def.IsMoss ? Harvester.harvestMode != HarvestMode.Moss : Harvester.harvestMode == HarvestMode.Moss);

        public override string GetReport()
        {
            return "TR_HarvestingReport".Translate(this.TargetA.Thing.def.LabelCap);
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            LocalTargetInfo target = job.GetTarget(TargetIndex.A);
            if (target.IsValid && !pawn.Reserve(target, job, 1, -1, null, errorOnFailed))
            {
                return false;
            }
            pawn.ReserveAsManyAsPossible(job.GetTargetQueue(TargetIndex.A), job);
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
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
                    //Time based on each value per Tick 
                    ticksToHarvest =  (int)Math.Round((TiberiumCrystal.HarvestValue / Harvester.kindDef.harvestValue), MidpointRounding.AwayFromZero);
                    //Ticks Needed to get 1 single value stored
                    ticksPerValue = (int) (ticksToHarvest / TiberiumCrystal.HarvestValue);
                    //Growth removed whenever value is added
                    growthPerValue = (TiberiumCrystal.Growth / (float) ticksToHarvest) * ticksPerValue;

                },
                tickAction = delegate
                {
                    if (Harvester.Container.CapacityFull)
                    {
                        EndJobWith(JobCondition.InterruptForced);
                        return;
                    }

                    ticksPassed++;
                    if (ticksPassed > ticksToHarvest)
                    {
                        TiberiumCrystal.Harvested();
                        ticksPassed = 0;
                        ReadyForNextToil();
                        return;
                    }

                    if (ticksPassed % ticksPerValue == 0)
                    {
                        TiberiumCrystal.Harvest(Harvester, growthPerValue);
                    }
                }
            };
            harvest.AddFinishAction(() => Harvester.TNWManager.ReservationManager.Dequeue(TiberiumCrystal, Harvester));
            harvest.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            harvest.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
            harvest.FailOn(() => FailOn);
            harvest.WithEffect(EffecterDefOf.Harvest, TargetIndex.A);
            harvest.defaultCompleteMode = ToilCompleteMode.Never;
            yield return harvest;
            yield return Toils_Jump.Jump(extractTarget);
            yield break;
        }
    }
}