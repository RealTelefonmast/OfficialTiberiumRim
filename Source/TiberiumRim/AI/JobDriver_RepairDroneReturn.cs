using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace TR;

public class ThinkNode_ConditionalRepair : ThinkNode_Conditional
{
    public override bool Satisfied(Pawn pawn)
    {
        var drone = pawn as RepairDrone;
        var comp = drone.parentComp;
        if (!comp.AnyMechAvailableForRepair) return false;
        return pawn.CurJobDef != TRCDefOf.RepairMechanicalPawn;
    }
}

public class ThinkNode_ConditionalNoStation : ThinkNode_Conditional
{
    public override bool Satisfied(Pawn pawn)
    {
        return pawn is RepairDrone { HasNoStation: true };
    }
}

public class JobGiver_RepairDroneReturn : ThinkNode_JobGiver
{
    public override ThinkResult TryIssueJobPackage(Pawn pawn, JobIssueParams jobParams)
    {
        if (pawn is RepairDrone drone)
        {
            if (drone.HasNoStation)
            {
                return ThinkResult.NoJob;
            }
            
            if (drone.OutsideOfStationRadius)
            {
                var jobDef = TRCDefOf.ReturnFromRepair;
                var job = JobMaker.MakeJob(jobDef, drone.ParentBuilding);
                return new ThinkResult(job, this);
            }
        }
        
        
        return base.TryIssueJobPackage(pawn, jobParams);
    }

    public override Job TryGiveJob(Pawn pawn)
    {
        var jobDef = TRCDefOf.ReturnFromRepair;
        var drone = pawn as RepairDrone;
        var job = JobMaker.MakeJob(jobDef, drone.ParentBuilding);
        return job;
    }
}

public class JobDriver_RepairDroneReturn : JobDriver
{
    private RepairDrone Drone => this.pawn as RepairDrone;
    private Comp_DroneStation Comp => Drone.parentComp;

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return true;
    }

    public override IEnumerable<Toil> MakeNewToils()
    {
        yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.OnCell);
        Toil repair = new Toil();
        repair.initAction = delegate
        {
            Comp.StoreDrone(Drone);
        };
        yield return repair;
    }
}