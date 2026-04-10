using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace TiberiumRim
{
    public class Designator_Extended : Designator
    {
        protected bool mustBeUsed = false;

        public virtual bool MustStaySelected => mustBeUsed;

        public override AcceptanceReport CanDesignateCell(IntVec3 loc)
        {
            throw new NotImplementedException();
        }
    }
}
