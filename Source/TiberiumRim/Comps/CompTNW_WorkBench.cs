using System.Collections.Generic;
using Verse;

namespace TiberiumRim;

public class CompTNW_WorkBench : CompTNW
{
    public override IEnumerable<IntVec3> InnerConnectionCells
    {
        get
        {
            var rect = parent.OccupiedRect();
            var rot = parent.Rotation;
            if (rot == Rot4.North) return rect.RemoveCorners(new[] { 1, 2 });

            if (rot == Rot4.East) return rect.RemoveCorners(new[] { 2, 3 });

            if (rot == Rot4.South) return rect.RemoveCorners(new[] { 3, 4 });

            return rect.RemoveCorners(new[] { 4, 1 });
        }
    }
}