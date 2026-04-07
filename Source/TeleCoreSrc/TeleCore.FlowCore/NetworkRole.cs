using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeleCore
{
    [Flags]
    public enum NetworkRole
    {
        Controller = 1,
        Transmitter = 2,
        Producer = 4,
        Consumer = 8,
        Storage = 16,
        Requester = 32,
        All = 64,

        AllContainers = Producer | Consumer | Storage,
        //All = Transmitter | AllContainers,
    }
}
