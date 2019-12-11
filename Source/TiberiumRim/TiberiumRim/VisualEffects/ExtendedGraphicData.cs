using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class ExtendedGraphicData
    {
        public bool alignToBottom = false;
        public bool rotateDrawSize = true;
        public bool drawRotated = true;
        public Vector3 drawOffset = Vector3.zero;
        public List<string> linkStrings;
    }
}
