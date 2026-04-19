using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace TR;

public class JobDriver_DoMechConstructionBill : JobDriver
{
    //Steps:
    //Mech recipe selected (ie build harvester)
    //Recipies can be queued
    //Hangar awaits resources (for recipe bill)
    //Available hauler pawn should haul resources from available stockpiles
    //Once all resources are equal to required resources (keep internal counter, defvaluestack?)
    //Begin automatic timed construction, spawn mech on the center, draft it, move it off to the front of the hangar awaiting further instruction

    public Building_Hangar BillGiver => (Building_Hangar)job.GetTarget(TargetIndex.A).Thing;

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        if (!pawn.Reserve(job.GetTarget(TargetIndex.A), job, 1, -1, null, errorOnFailed)) return false;
        if (!pawn.Reserve(job.GetTarget(TargetIndex.B), job, 1, -1, null, errorOnFailed)) return false;
        pawn.ReserveAsManyAsPossible(job.GetTargetQueue(TargetIndex.B), job);
        return true;
    }

    public override IEnumerable<Toil> MakeNewToils()
    {
        AddEndCondition(delegate
        {
            var thing = GetActor().jobs.curJob.GetTarget(TargetIndex.A).Thing;
            if (thing is Building && !thing.Spawned) return JobCondition.Incompletable;
            return JobCondition.Ongoing;
        });
        this.FailOnBurningImmobile(TargetIndex.A);

        //Haul all resources to the hangar
        var getToHaulTarget =
            Toils_Goto.GotoThing(TargetIndex.B,
                PathEndMode.Touch); //.FailOnSomeonePhysicallyInteracting(TargetIndex.B);
        var startCarryingThing = Toils_Haul.StartCarryThing(TargetIndex.B, false, true);
        var jumpIfAlsoCollectingNextTarget =
            Toils_Haul.JumpIfAlsoCollectingNextTargetInQueue(getToHaulTarget, TargetIndex.B);

        yield return getToHaulTarget;
        yield return startCarryingThing;
        //yield return jumpIfAlsoCollectingNextTarget;
        yield return
            StartHaulToHangar(); //Just starts walking, which isnt waiting until pawn arrives, which allows the next toil to collect things on the way
        //Any new job with pathing will simply queue a subroutine to be done before the final pathing job
        //yield return Toils_Jump.JumpIf(jumpIfAlsoCollectingNextTarget, () => this.pawn.IsCarryingThing(this.job.GetTarget(TargetIndex.B).Thing));

        yield return PlaceHauledResourceAndConsume(BillGiver, pawn);
    }

    private static Toil StartHaulToHangar()
    {
        var gotoDest = ToilMaker.MakeToil("CarryHauledThingToHangar");
        gotoDest.initAction = delegate
        {
            gotoDest.actor.pather.StartPath(gotoDest.actor.jobs.curJob.targetA.Thing, PathEndMode.Touch);
        };
        gotoDest.AddFailCondition(delegate
        {
            var thing = gotoDest.actor.jobs.curJob.targetA.Thing;
            if (thing.Destroyed ||
                (!gotoDest.actor.jobs.curJob.ignoreForbidden && thing.IsForbidden(gotoDest.actor))) return true;
            return false;
        });
        gotoDest.defaultCompleteMode = ToilCompleteMode.PatherArrival;
        return gotoDest;
    }

    private static Toil PlaceHauledResourceAndConsume(Building_Hangar hangar, Pawn byPawn)
    {
        var toil = ToilMaker.MakeToil();
        toil.initAction = delegate
        {
            var actor = toil.actor;
            var curJob = actor.jobs.curJob;
            if (actor.carryTracker.CarriedThing == null)
            {
                Log.Error(actor + " tried to place hauled thing in cell but is not hauling anything.");
                return;
            }

            var placedAction = delegate(Thing thing, int count)
            {
                hangar.Bills.Notify_AddedResources(thing, count);
                byPawn.Map.designationManager.RemoveAllDesignationsOn(thing);
                thing.DeSpawnOrDeselect();
            };

            Thing thing;
            if (!actor.carryTracker.TryDropCarriedThing(hangar.Position, ThingPlaceMode.Direct, out thing,
                    placedAction))
            {
            }
        };
        return toil;
    }
}