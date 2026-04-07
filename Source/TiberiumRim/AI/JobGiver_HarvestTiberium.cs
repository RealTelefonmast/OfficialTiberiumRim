using System;
using System.Collections.Generic;
using RimWorld;
using TR.TiberiumObjects;
using TR.TiberiumProcessing;
using TR.Util;
using Verse;
using Verse.AI;

namespace TR.AI;

public class JobGiver_HarvestTiberium : ThinkNode_JobGiver
{
    public override Job TryGiveJob(Pawn pawn)
    {
        var harvester = pawn as Harvester;

        if (harvester.CurrentPriority != HarvesterPriority.Harvest) return null;
        if (harvester.IsHarvesting) return null;

        var crystal = pawn.Map.Tiberium().HarvesterInfo.FindClosestTiberiumFor(harvester);
        if (crystal == null) return null;

        var job = JobMaker.MakeJob(Defs.TiberiumDefOf.HarvestTiberium, crystal);

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
    private float growthPerValue;
    private int ticksPassed;
    private int ticksPerValue;
    private int ticksToHarvest;

    private TiberiumCrystal TiberiumCrystal => TargetA.Thing as TiberiumCrystal;

    private Harvester Harvester => pawn as Harvester;

    private bool FailOn => TiberiumProcessing.Harvester.PlayerInterrupt || !TiberiumProcessing.Harvester.CanHarvestTiberium(TiberiumObjects.TiberiumCrystal.def);

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

        private bool FailOn => TiberiumProcessing.Harvester.PlayerInterrupt || !TiberiumProcessing.Harvester.CanHarvestTiberium(TiberiumObjects.TiberiumCrystal.def);

        public override string GetReport()
        {
            return "TR_HarvestingReport".Translate(this.TargetA.Thing.def.LabelCap);
        }

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        var target = job.GetTarget(TargetIndex.A);
        if (target.IsValid && !pawn.Reserve(target, job, 1, -1, null, errorOnFailed)) return false;
        pawn.ReserveAsManyAsPossible(job.GetTargetQueue(TargetIndex.A), job);
        return true;
    }

    public override IEnumerable<Toil> MakeNewToils()
    {
        yield return Toils_JobTransforms.MoveCurrentTargetIntoQueue(TargetIndex.A);
        var extractTarget = Toils_JobTransforms.ClearDespawnedNullOrForbiddenQueuedTargets(TargetIndex.A);
        yield return extractTarget;
        yield return Toils_JobTransforms.SucceedOnNoTargetInQueue(TargetIndex.A);
        yield return Toils_JobTransforms.ExtractNextTargetFromQueue(TargetIndex.A);

        yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch)
            .JumpIfDespawnedOrNullOrForbidden(TargetIndex.A, extractTarget);

        var harvest = new Toil
        {
            initAction = delegate
            {
                //Time based on each weight per Tick 
                ticksToHarvest = (int)Math.Round(TiberiumObjects.TiberiumCrystal.HarvestValue / TiberiumProcessing.Harvester.kindDef.harvestValue,
                    MidpointRounding.AwayFromZero);
                //Ticks Needed to get 1 single weight stored
                ticksPerValue = (int)(ticksToHarvest / TiberiumObjects.TiberiumCrystal.HarvestValue);
                //Growth removed whenever weight is added
                growthPerValue = TiberiumObjects.TiberiumCrystal.Growth / ticksToHarvest * ticksPerValue;
            },
            tickAction = delegate
            {
                if (TiberiumProcessing.Harvester.Container.CapacityFull)
                {
                    EndJobWith(JobCondition.InterruptForced);
                    return;
                }

                if (ticksPassed > ticksToHarvest)
                {
                    if (TiberiumObjects.TiberiumCrystal.Spawned && !TiberiumProcessing.Harvester.Container.CapacityFull) TiberiumObjects.TiberiumCrystal.DeSpawn();
                    ticksPassed = 0;
                    ReadyForNextToil();
                    return;
                }

                    if (ticksPassed % ticksPerValue == 0)
                    {
                        TiberiumProcessing.Harvester.Animator.Start("Harvest", true);
                        TiberiumObjects.TiberiumCrystal.Harvest(Harvester, growthPerValue);
                    }
                    ticksPassed++;
                }
            };
            //harvest.AddFinishAction(() => Harvester.TNWManager.ReservationManager.Dequeue(TiberiumCrystal, Harvester));
            harvest.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            harvest.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
            harvest.FailOn(() => FailOn);
            harvest.WithEffect(EffecterDefOf.Harvest, TargetIndex.A);
            harvest.defaultCompleteMode = ToilCompleteMode.Never;
            harvest.AddFinishAction(TiberiumProcessing.Harvester.Animator.Stop);
            yield return harvest;
            yield return Toils_Jump.Jump(extractTarget);
        }
    }
}