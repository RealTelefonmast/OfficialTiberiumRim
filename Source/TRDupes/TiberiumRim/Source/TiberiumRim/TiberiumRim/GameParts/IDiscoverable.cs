using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TiberiumRim
{
    public interface IDiscoverable
    {
        string DiscoverTag { get; }
        bool Discovered { get; }

        string DiscoveredLabel { get; }
        string UnknownLabel { get; }
        string DiscoveredDescription { get; }
        string UnknownDescription { get; }
        string DescriptionExtra { get; }
    }
}
