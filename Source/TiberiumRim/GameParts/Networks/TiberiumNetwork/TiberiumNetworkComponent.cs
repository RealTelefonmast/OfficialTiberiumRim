using TeleCore;
using TeleCore.FlowCore;

namespace TiberiumRim
{
    public class TiberiumNetworkSubPart : NetworkSubPart
    {
        public TiberiumNetworkSubPart(Comp_Network parent, NetworkSubPartProperties properties) : base(parent, properties)
        {
        }


        public override void Notify_ContainerStateChanged(NotifyContainerChangedArgs<NetworkValueDef> args)
        {
            base.Notify_ContainerStateChanged(args);
            if (args.Action == NotifyContainerChangedAction.Filled)
            {
                if (NetworkRole.HasFlag(NetworkRole.Producer))
                    GameComponent_EVA.EVAComp().ReceiveSignal(EVASignal.SilosNeeded, Parent.Thing);
            }
        }
    }
}
