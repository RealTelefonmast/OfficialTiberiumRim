using System.Collections.Generic;
using Verse;

namespace TiberiumRim
{
    public class HediffWithGizmos : HediffWithComps
    {
        public virtual IEnumerable<Gizmo> GetGizmos()
        {
            yield break;
        }
    }
}
