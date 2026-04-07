using TeleCore.Network;

namespace TR.GameParts.Networks.TiberiumNetwork;

public class TiberiumNetworkSubPart : NetworkPart
{
    //TODO: Move to globalevent

    // public override void Notify_ContainerStateChanged(NotifyContainerChangedArgs<NetworkValueDef> args)
    // {
    //     base.Notify_ContainerStateChanged(args);
    //     if (args.Action == NotifyContainerChangedAction.Filled)
    //     {
    //         if (NetworkRole.HasFlag(NetworkRole.Producer))
    //             GameComponent_EVA.EVAComp().ReceiveSignal(EVASignal.SilosNeeded, Parent.Thing);
    //     }
    // }
}