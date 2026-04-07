using System.Collections.Generic;
using Verse;

namespace TeleCore.Hediffs;

public class HediffComp_Gizmo : HediffComp
{
    public virtual IEnumerable<Gizmo> GetGizmos()
    {
        yield return null;
    }
}