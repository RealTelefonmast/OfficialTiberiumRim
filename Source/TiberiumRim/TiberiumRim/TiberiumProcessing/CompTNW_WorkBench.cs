using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace TiberiumRim
{
    public class CompTNW_WorkBench : CompTNW
    {
        public override IEnumerable<IntVec3> InnerConnectionCells
        {
            get
            {
                var rect = parent.OccupiedRect();
                var rot = parent.Rotation;
                if (rot == Rot4.North)
                {
                    return rect.RemoveCorners(new int[] { 1, 2 });
                }
                else if (rot == Rot4.East)
                {
                    return rect.RemoveCorners(new int[] { 2, 3 });
                }
                else if (rot == Rot4.South)
                {
                    return rect.RemoveCorners(new int[] { 3, 4 });
                }
                else
                {
                    return rect.RemoveCorners(new int[] { 4, 1 });
                }
            }
        }
    }
}
