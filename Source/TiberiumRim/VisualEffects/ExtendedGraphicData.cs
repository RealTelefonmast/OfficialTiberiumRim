using System.Collections.Generic;
using UnityEngine;

namespace TiberiumRim
{
    public class ExtendedGraphicData
    {
        public bool alignToBottom = false;
        public bool rotateDrawSize = true;
        public bool? drawRotatedOverride = null;
        public Vector3 drawOffset = Vector3.zero;
        public List<string> linkStrings;
    }
}
