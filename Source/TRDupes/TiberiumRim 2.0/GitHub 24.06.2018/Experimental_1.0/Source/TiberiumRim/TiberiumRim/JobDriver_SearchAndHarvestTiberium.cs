using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;
using RimWorld;

namespace TiberiumRim
{
    public class JobGiver_SearchAndHarvestTiberium : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            Harvester harvester = pawn as Harvester;
            bool erFlag = harvester.ShouldEradicate;
            if (harvester.ShouldHarvest && !harvester.Harvesting && !harvester.Unloading)
            {
                TraverseParms traverseParms = TraverseParms.For(harvester, Danger.Some, TraverseMode.PassDoors, false);
                TiberiumCrystal tiberiumTarget = TRUtils.ClosestPreferableReachableAndReservableTiberiumForHarvester(harvester, harvester.Position, harvester.Map, harvester.TiberiumDefToPrefer, !erFlag, traverseParms, PathEndMode.Touch);
                if (tiberiumTarget != null)
                {
                    if (harvester.CanReach(tiberiumTarget, PathEndMode.Touch, Danger.Some, false))
                    {
                        JobDef job = DefDatabase<JobDef>.GetNamed("SearchAndHarvestTiberium");
                        return new Job(job, tiberiumTarget);
                    }
                }
                if (harvester.shouldStopHarvesting == false)
                {
                    harvester.shouldStopHarvesting = true;
                }
            }
            return null;
        }
    }

    public class JobDriver_SearchAndHarvestTiberium : JobDriver
    {       
        protected TiberiumCrystal Tiberium
        {
            get
            {
                return (TiberiumCrystal)this.job.targetA.Thing;
            }
        }

        protected Harvester Harvester
        {
            get
            {
                return this.pawn as Harvester;
            }
        }

        private int ticksPassed;

        private int newTicks;
        private float growth;

        public bool IsEradicating
        {
            get
            {
                return !Tiberium.def.HarvestableType;
            }
        }

        public bool FailOn
        {
            get
            {
                return Harvester.shouldStopHarvesting || IsEradicating ? !Harvester.ShouldEradicate : Harvester.ShouldEradicate;
            }
        }

        public override bool TryMakePreToilReservations()
        {
            if (pawn.CanReserve(this.TargetA))
            {
                return this.pawn.Reserve(this.TargetA, this.job);
            }
            return false;
        }

        public override string GetReport()
        {
            string report = Harvester.ShouldEradicate ? "EradicateMoss".Translate() : "HarvestingReport".Translate(new object[] { this.TargetA.Thing.def.LabelCap });
            return "HarvestingReport".Translate(new object[] { this.TargetA.Thing.def.LabelCap });
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref ticksPassed, "ticksPassed");
            Scribe_Values.Look<int>(ref newTicks, "newTicks");
            Scribe_Values.Look<float>(ref growth, "growth");
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            Toil gotoToil = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            gotoToil.FailOn(() => FailOn);
            yield return gotoToil;

            Toil harvest = new Toil();
            harvest.initAction = delegate
            {
                growth += Tiberium.growthInt;
                newTicks += (int)(Tiberium.harvestTicks * growth);
            };
            harvest.tickAction = delegate
            {
                Harvester actor = (Harvester)harvest.actor;
                if (!actor.Container.CapacityFull)
                {
                    if (ticksPassed < newTicks)
                    {
                        float value = (growth / newTicks) * Tiberium.def.tiberium.maxHarvestValue;                     
                        if (actor.Container.AddCrystal(Tiberium.def.TibType, value, out float value2))
                        {
                            // growth reduction adjusted to excess value
                            Tiberium.growthInt -= (growth / newTicks) * (1 -(value2/value));
                        }
                    }
                    else
                    {
                        if (Tiberium != null)
                        {
                            Tiberium.Destroy();
                        }
                        this.EndJobWith(JobCondition.InterruptOptional);
                        return;
                    }
                }
                else
                {
                    this.EndJobWith(JobCondition.InterruptForced);
                    return;
                }
                ticksPassed++;
            };
            harvest.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            harvest.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
            harvest.FailOn(() => FailOn);
            harvest.defaultCompleteMode = ToilCompleteMode.Never;
            yield return harvest;
        }
    }
}
