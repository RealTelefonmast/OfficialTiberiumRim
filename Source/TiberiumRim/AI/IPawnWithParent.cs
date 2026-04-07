using System.Collections.Generic;
using Verse;

namespace TR.AI;

public interface IPawnWithParent
{
    List<IntVec3> Field { get; }
    ThingWithComps Parent { get; }
    bool CanWander { get; }
}