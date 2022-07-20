using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeleCore;

namespace TiberiumRim
{
    public class TiberiumNetworkSubPart : NetworkSubPart
    {
        public TiberiumNetworkSubPart(Comp_NetworkStructure parent, NetworkSubPartProperties properties) : base(parent, properties)
        {
        }

        public override void Notify_ContainerFull()
        {
            if (NetworkRole.HasFlag(NetworkRole.Producer))
                GameComponent_EVA.EVAComp().ReceiveSignal(EVASignal.SilosNeeded, Parent.Thing);
        }
    }
}
