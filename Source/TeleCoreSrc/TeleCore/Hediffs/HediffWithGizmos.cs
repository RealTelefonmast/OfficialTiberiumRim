using System.Collections.Generic;
using Verse;

namespace TeleCore.Unsorted;

public class HediffWithGizmos : HediffWithComps
{
    public virtual IEnumerable<Gizmo> GetGizmos()
    {
        yield break;
    }
}