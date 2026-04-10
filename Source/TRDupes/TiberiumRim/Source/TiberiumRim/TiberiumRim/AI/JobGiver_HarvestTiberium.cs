using System;
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
                TiberiumCrystal crystal = harvester.HarvestTarget;
                if (crystal != null)
                {
                    if (harvester.CanReserveAndReach(crystal, PathEndMode.Touch, Danger.Deadly))
                    {
                        return JobMaker.MakeJob(TiberiumDefOf.HarvestTiberium, crystal);
                    }
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

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return Harvester.Reserve(TargetA, job);
        }

        public override string GetReport()
        {
            return "TR_HarvestingReport".Translate(this.TargetA.Thing.def.LabelCap);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            Toil gotoToil = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            gotoToil.FailOn(() => FailOn);
            yield return gotoToil;

            Toil harvest = new Toil
            {
                initAction = delegate
                {
                    ticksToHarvest += TiberiumCrystal.HarvestTime;
                    ticksPerValue = (int) (ticksToHarvest / TiberiumCrystal.HarvestValue);
                    growthPerValue = (TiberiumCrystal.Growth / (float) ticksToHarvest) * ticksPerValue;
                },
                tickAction = delegate
                {
                    if (!Harvester.Container.CapacityFull)
                    {
                        if (ticksPassed < ticksToHarvest)
                        {
                            if (ticksPassed % ticksPerValue == 0)
                            {
                                if (Harvester.Container.TryAddValue(TiberiumCrystal.def.TiberiumValueType, 1, out float actualValue))
                                {
                                    TiberiumCrystal.Harvest(growthPerValue * (actualValue / 1));
                                }
                            }
                        }
                        else
                        {
                            if (TiberiumCrystal.Growth <= 0.01f)
                            {
                                TiberiumCrystal.DeSpawn();
                            }

                            EndJobWith(JobCondition.Succeeded);
                        }
                        ticksPassed++;
                    }
                    else
                    {
                        EndJobWith(JobCondition.Succeeded);
                    }
                }
            };
            harvest.AddFinishAction(() => Harvester.TNWManager.ReservationManager.UnreserveFor(TiberiumCrystal, Harvester));
            harvest.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            harvest.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
            harvest.FailOn(() => FailOn);
            harvest.defaultCompleteMode = ToilCompleteMode.Never;
            yield return harvest;          
        }
    }
}
