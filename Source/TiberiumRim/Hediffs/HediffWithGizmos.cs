using System.Collections.Generic;
using Verse;

namespace TR.Hediffs;

public class HediffWithGizmos : HediffWithComps
{
    public virtual IEnumerable<Gizmo> GetGizmos()
    {
        yield break;
    }
}