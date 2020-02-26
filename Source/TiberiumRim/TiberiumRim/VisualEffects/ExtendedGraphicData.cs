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
        public bool repeatSprite = false;
        public int spriteTicks = 10;
        public Vector3 drawOffset = Vector3.zero;
        public List<string> linkStrings;
    }
}
