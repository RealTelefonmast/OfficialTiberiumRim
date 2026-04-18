using System.Collections.Generic;
using TeleCore.CompProperties;
using Verse;

namespace TeleCore.Unsorted;

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
