using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TiberiumRim
{
    public interface IContainerHolder
    {
        void Notify_ContainerFull();

        bool ShouldNotifyEVA { get; }
        TiberiumContainer Container { get; }
    }
}
