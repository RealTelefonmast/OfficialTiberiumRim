using System;
using System.Collections;
using System.Collections.Generic;

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
