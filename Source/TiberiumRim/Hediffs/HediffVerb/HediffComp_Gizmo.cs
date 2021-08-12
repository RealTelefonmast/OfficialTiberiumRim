using System.Collections.Generic;
using Verse;

namespace TiberiumRim
{
    public class HediffComp_Gizmo : HediffComp
    {
        public virtual IEnumerable<Gizmo> GetGizmos()
        {
            yield return null;
        }
    }
}
