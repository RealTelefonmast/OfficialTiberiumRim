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
            if (harvester.ShouldHarvest && !(harvester.CurJob?.def == TiberiumDefOf.HarvestTiberium))
            {
                Find.CameraDriver.StartCoroutine(harvester.CalculateTiberium());
                TiberiumCrystal crystal = harvester.HarvestTarget;
                if (crystal != null)
                {
                    if (harvester.CanReserveAndReach(crystal, PathEndMode.ClosestTouch, Danger.Deadly))
                    {
                        return new Job(TiberiumDefOf.HarvestTiberium, crystal);
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

        private TiberiumCrystal TiberiumCrystal
        {
            get
            {
                return TargetA.Thing as TiberiumCrystal;
            }
        }

        private Harvester Harvester
        {
            get
            {
                return pawn as Harvester;
            }
        }

        private bool FailOn
        {
            get
            {
                return Harvester.idleAtRefinery || (TiberiumCrystal.def.IsMoss ? Harvester.harvestMode != HarvestMode.Moss : Harvester.harvestMode == HarvestMode.Moss);
            }
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return Harvester.Reserve(TargetA, job);
        }

        public override string GetReport()
        {
            string report = TiberiumCrystal.def.IsMoss ? "EradicateMoss".Translate() : "HarvestingReport".Translate(this.TargetA.Thing.def.LabelCap);
            return "HarvestingReport".Translate(this.TargetA.Thing.def.LabelCap);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            Toil gotoToil = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            gotoToil.FailOn(() => FailOn);
            yield return gotoToil;

            Toil harvest = new Toil();
            harvest.initAction = delegate
            {
                Log.Message(TiberiumCrystal.HarvestTime + " | " + TiberiumCrystal.HarvestValue + " | " + TiberiumCrystal.Growth);
                if(TiberiumCrystal.HarvestValue < 1f)
                {
                    TiberiumCrystal.DeSpawn();
                    return;
                }
                ticksToHarvest += TiberiumCrystal.HarvestTime;
                ticksPerValue = ticksToHarvest / (int)TiberiumCrystal.HarvestValue;
                growthPerValue = TiberiumCrystal.Growth / (float)ticksPerValue;
                //valuePerTick = TiberiumCrystal.HarvestValue / (float)ticksToHarvest;
                //growthPerTick = TiberiumCrystal.growth / (float)ticksToHarvest;
            };
            harvest.tickAction = delegate
            {
                if (!Harvester.Container.CapacityFull)
                {
                    if (ticksPassed < ticksToHarvest)
                    {
                        if (ticksPassed % ticksPerValue == 0)
                        {
                            if (Harvester.Container.TryAddValue(TiberiumCrystal.def.TiberiumType, 1, out int excess))
                            {
                                TiberiumCrystal.growth -= growthPerValue - (excess / TiberiumCrystal.def.tiberium.harvestValue);
                            }
                        }
                    }
                    else
                    {
                        TiberiumCrystal.DeSpawn();
                        EndJobWith(JobCondition.Succeeded);
                    }
                    ticksPassed++;
                }
                else
                {                  
                    EndJobWith(JobCondition.Succeeded);
                }
            };
            harvest.AddFinishAction(() => Harvester.HarvestTarget = null);
            harvest.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            harvest.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
            harvest.FailOn(() => FailOn);
            harvest.defaultCompleteMode = ToilCompleteMode.Never;
            yield return harvest;          
        }

    }
}
