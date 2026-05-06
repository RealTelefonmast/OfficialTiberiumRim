using System.Collections.Generic;
using TeleCore.Types;
using Verse;

namespace TeleCore.Defs;

public class FXThingDef : ThingDef
{
    public ExtendedGraphicData extraData = new();
    public GraphicData graphicData2;

    public override IEnumerable<string> ConfigErrors()
    {
        foreach (var error in base.ConfigErrors()) yield return error;

        var fxComp = GetCompProperties<CompProperties_FX>();
        if (fxComp != null)
            foreach (var overlay in fxComp.overlays)
            {
            }
    }
}