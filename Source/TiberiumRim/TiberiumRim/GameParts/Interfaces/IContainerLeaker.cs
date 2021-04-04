using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TiberiumRim
{
    public interface IContainerLeaker
    {
        bool ShouldLeak { get; }
        TiberiumContainer Container { get; }
    }
}
