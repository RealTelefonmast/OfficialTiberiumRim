using System;

namespace TeleCore.FlowCore;

[Flags]
public enum NetworkRole
{
    Controller = 1,
    Transmitter = 2,
    Passthrough = 4,
    Producer = 8,
    Consumer = 16,
    Storage = 32,
    Requester = 64,
    Valve = 128,
    Pump = 256,
    EndPoint = 512,
    All = 127,

    AllContainers = Producer | Consumer | Storage | Requester,

    AllFlag = Controller | Transmitter | Passthrough | Valve | AllContainers
    //All = 127
}