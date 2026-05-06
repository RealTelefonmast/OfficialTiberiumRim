using System;
using UnityEngine;
using Verse;

namespace TeleCore;

//Unusable until further notice
[Obsolete]
public class Graphic_LinkedNoPadding : Graphic_Linked
{
    public override Material MatSingle =>
        MaterialAtlasPool_TR.SubMaterialFromAtlas(subGraphic.MatSingle, LinkDirections.None);

    public override Material LinkedDrawMatFrom(Thing parent, IntVec3 cell)
    {
        var num = 0;
        var num2 = 1;
        for (var i = 0; i < 4; i++)
        {
            var c = cell + GenAdj.CardinalDirections[i];
            if (ShouldLinkWith(c, parent)) num += num2;
            num2 *= 2;
        }

        var linkSet = (LinkDirections)num;
        return MaterialAtlasPool_TR.SubMaterialFromAtlas(subGraphic.MatSingleFor(parent), linkSet);
    }
}