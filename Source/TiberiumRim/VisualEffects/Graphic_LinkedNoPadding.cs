﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    //Unusable until further notice
    [Obsolete]
    public class Graphic_LinkedNoPadding : Graphic_Linked
    {
        public override Material MatSingle => MaterialAtlasPool_TR.SubMaterialFromAtlas(subGraphic.MatSingle, LinkDirections.None);

        protected override Material LinkedDrawMatFrom(Thing parent, IntVec3 cell)
        {
            int num = 0;
            int num2 = 1;
            for (int i = 0; i < 4; i++)
            {
                IntVec3 c = cell + GenAdj.CardinalDirections[i];
                if (this.ShouldLinkWith(c, parent))
                {
                    num += num2;
                }
                num2 *= 2;
            }
            LinkDirections linkSet = (LinkDirections)num;
            return MaterialAtlasPool_TR.SubMaterialFromAtlas(this.subGraphic.MatSingleFor(parent), linkSet);
        }
    }
}
