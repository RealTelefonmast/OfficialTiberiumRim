using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace TiberiumRim
{
    public interface IPawnWithParent
    {
        List<IntVec3> Field { get; }
        ThingWithComps Parent { get; }
        bool CanWander { get; }
    }
}
