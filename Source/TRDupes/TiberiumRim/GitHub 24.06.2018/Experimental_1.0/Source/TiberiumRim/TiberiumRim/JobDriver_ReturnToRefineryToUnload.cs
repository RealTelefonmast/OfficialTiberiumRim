using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Verse;
using Verse.AI;
using RimWorld;

namespace TiberiumRim
{
    public class JobGiver_ReturnToRefineryToUnload : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            Harvester harvester = pawn as Harvester;
            Building_Refinery refinery = harvester.AvailableRefineryToUnload;
            if (refinery != null)
            {
                if (harvester.ShouldUnload && !harvester.Unloading && harvester.CanReserve(refinery))
                {
                    JobDef job = DefDatabase<JobDef>.GetNamed("ReturnToRefineryToUnload");
                    return new Job(job, refinery);
                }
            }
            return null;
        }
    }

    public class JobDriver_ReturnToRefineryToUnload : JobDriver
    {
        private Building_Refinery Refinery
        {
            get
            {
                return (Building_Refinery)this.job.targetA.Thing;
            }
        }

        private Harvester Harvester
        {
            get
            {
                return (Harvester)this.pawn;
            }
        }

        private float CurrentStorage;

        private int ticksPassed;

        public override bool TryMakePreToilReservations()
        {
            return this.pawn.Reserve(this.TargetA, this.job);
        }

        public override void ExposeData()
        {           
            base.ExposeData();
            Scribe_Values.Look<int>(ref ticksPassed, "ticksPassed");
            Scribe_Values.Look<float>(ref CurrentStorage, "ticksPassed");
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            Toil gotoToil = Toils_Goto.GotoCell(Refinery.InteractionCell, PathEndMode.OnCell);
            gotoToil.FailOnDespawnedOrNull(TargetIndex.A);
            yield return gotoToil;
            Toil unload = new Toil();
            unload.initAction = delegate
            {
                Harvester harvester = unload.actor as Harvester;      
                
                CurrentStorage += harvester.Container.GetTotalStorage;
                harvester.pather.StopDead();
                harvester.Rotation = harvester.AvailableRefinery.Rotation.Opposite;
            };
            unload.tickAction = delegate
            {
                Harvester actor = unload.actor as Harvester;
                if (!Refinery.NetworkComp.Container.CapacityFull)
                {
                    if(ticksPassed < Refinery.refineTicks)
                    {
                        TiberiumType type = actor.Container.MainType;
                        float value = (CurrentStorage / Refinery.refineTicks);
                        if (Refinery.NetworkComp.Container.AddCrystal(type, value, out float excess))
                        {
                            actor.Container.RemoveCrystal(type, value - excess);
                        }
                    }
                    else
                    {
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
            unload.FailOnDespawnedOrNull(TargetIndex.A);
            unload.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
            unload.defaultCompleteMode = ToilCompleteMode.Never;
            yield return unload;
        }
    }
}
