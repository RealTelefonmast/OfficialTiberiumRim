using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.Noise;

namespace TiberiumRim
{
    public struct MultiCurvePoint
    {

    }

    public class MultiCurve : IEnumerable<MultiCurvePoint>, IEnumerable
    {
        IEnumerator<MultiCurvePoint> IEnumerable<MultiCurvePoint>.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
