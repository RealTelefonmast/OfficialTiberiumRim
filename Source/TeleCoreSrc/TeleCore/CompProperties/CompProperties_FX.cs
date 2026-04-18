using System.Collections.Generic;
using System.Linq;
using TeleCore.Comps;
using TeleCore.Unsorted;
using Verse;

namespace TeleCore.CompProperties;

public class CompProperties_FX : Verse.CompProperties
{
    public List<FXEffecterData> effectLayers = new();

    //Layers
    public List<FXLayerData> fxLayers = new();
    public IntRange tickOffset = new(0, 333);

    public CompProperties_FX()
    {
        compClass = typeof(CompFX);
    }

    public override IEnumerable<string> ConfigErrors(ThingDef def)
    {
        var hasFade = Enumerable.Any(fxLayers, o => o.fade != null);
        var hasNonStatic = Enumerable.Any(fxLayers, o => o.fxMode != FXMode.Static);
        if (def.drawerType == DrawerType.MapMeshOnly)
        {
            if (hasNonStatic)
                yield return $"{def} has dynamic overlays but is MapMeshOnly";
            if (hasFade)
                yield return $"{def} has overlays with fade effect but is MapMeshOnly";
        }

        if (Enumerable.Any(fxLayers, o => o.fade != null && o.fxMode == FXMode.Static))
            yield return $"{def} has static overlays with fade effect";
    }
}