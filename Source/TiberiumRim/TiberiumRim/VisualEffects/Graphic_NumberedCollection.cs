﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace TiberiumRim
{
    public class Graphic_NumberedCollection : Graphic_Collection
    {
        public override void Init(GraphicRequest req)
        {
            base.Init(req);
        }

        public int Count => subGraphics.Length;

        public Graphic[] Graphics => subGraphics;
    }
}
