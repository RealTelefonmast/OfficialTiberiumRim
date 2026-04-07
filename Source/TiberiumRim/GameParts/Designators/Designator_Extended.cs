using System;
using Verse;

namespace TR.GameParts.Designators;

public class Designator_Extended : Designator
{
    protected bool mustBeUsed = false;

    public virtual bool MustStaySelected => mustBeUsed;

    public override AcceptanceReport CanDesignateCell(IntVec3 loc)
    {
        throw new NotImplementedException();
    }
}