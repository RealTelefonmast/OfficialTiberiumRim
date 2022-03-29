using System;

namespace TiberiumRim
{
    [Flags]
    public enum NetworkRole : uint
    {
        All = 0U,
        Controller  = 1U,
        Transmitter = 2U,
        Producer    = 4U,
        Consumer    = 8U,
        Storage     = 16U,
        Requester   = 32U,
        None = 64U,

        AllContainers = Producer | Consumer | Storage,
        //All = Transmitter | AllContainers,
    }
}
