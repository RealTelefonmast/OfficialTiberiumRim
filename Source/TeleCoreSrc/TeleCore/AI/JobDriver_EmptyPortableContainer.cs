using System.Collections.Generic;
using TeleCore.Comps;
using TeleCore.Things;
using TeleCore.Types.Enums;
using TeleCore.Types.Exposables;
using TeleCore.Types.Interfaces;
using Verse;
using Verse.AI;

namespace TeleCore.AI;

public class JobDriver_EmptyPortableContainer : JobDriver
{
    //A - Container
    private PortableNetworkContainer PortableNetworkContainer => TargetA.Thing as PortableNetworkContainer;

    //B - Network
    private ThingWithComps NetworkParent => TargetB.Thing as ThingWithComps;

    private NetworkVolume Container => PortableNetworkContainer.NetworkVolume;
    private CompNetwork NetworkComp => NetworkParent.GetComp<CompNetwork>();
    private INetworkPart NetworkPart => NetworkComp[PortableNetworkContainer.NetworkDef];
    private NetworkVolume TargetContainer => NetworkPart.Volume;

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return pawn.Reserve(TargetA, job);
    }

    private JobCondition TransferToContainer()
    {
        if (PortableNetworkContainer.NetworkVolume.FillState == ContainerFillState.Empty) return JobCondition.Succeeded;
        if (TargetContainer.FillState == ContainerFillState.Full) return JobCondition.Incompletable;

        for (var i = Container.Stack.Length - 1; i >= 0; i--)
        {
            var type = Container.Stack[i];
            //TODO: re-add process
            // if (!NetworkPart.NeedsValue(type, NetworkRole.Storage)) continue;
            // if (Container.TryTransferValue(NetworkPart.Container, type, 1, out _))
            // {
            //     //NetworkPart.Notify_ReceivedValue();
            // }
        }

        return JobCondition.None;
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDestroyedOrNull(TargetIndex.A);
        this.FailOnDestroyedOrNull(TargetIndex.B);
        this.FailOnBurningImmobile(TargetIndex.A);

        yield return Toils_General.DoAtomic(delegate { job.count = 1; });

        var reserveTargetA = Toils_Reserve.Reserve(TargetIndex.A);
        yield return reserveTargetA;

        //Goto portable container
        yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch);
        //Start carrying container
        yield return Toils_Haul.StartCarryThing(TargetIndex.A, false, true);

        //Carry container to network structure
        yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.Touch);

        //Start emptying
        var emptyContainer = new Toil();
        emptyContainer.tickAction = () =>
        {
            var condition = TransferToContainer();
            if (condition is JobCondition.Succeeded or JobCondition.Incompletable)
            {
                EndJobWith(condition);
                PortableNetworkContainer.Notify_FinishEmptyingToTarget();
            }
        };
        emptyContainer.WithProgressBar(TargetIndex.A, () => PortableNetworkContainer.EmptyPercent);
        emptyContainer.defaultCompleteMode = ToilCompleteMode.Never;
        yield return emptyContainer;
    }
}