using System.Collections.Generic;
using Verse;

namespace TeleCore.FlowCore;

public class CompProperties_NetworkBillsCrafter : CompProperties_Network
{
    [Unsaved] private List<CustomRecipePresetDef> presetDefsInt;

    public List<string> presetTags;

    [Unsaved] private List<CustomRecipeRatioDef> ratioDefsInt;

    //Tags are used to get lists of associated defs
    public List<string> ratioTags;

    public CompProperties_NetworkBillsCrafter()
    {
        compClass = typeof(Comp_NetworkBillsCrafter);
    }

    public List<CustomRecipeRatioDef> UsedRatioDefs
    {
        get
        {
            if (ratioDefsInt == null)
            {
                ratioDefsInt = new List<CustomRecipeRatioDef>();
                foreach (var ratioTag in ratioTags)
                {
                    var list = CustomNetworkRecipeReferences.TryGetRatiosOfTag(ratioTag);
                    if (list == null) continue;
                    ratioDefsInt.AddRange(list);
                }
            }

            return ratioDefsInt;
        }
    }

    public List<CustomRecipePresetDef> UsedPresetDefs
    {
        get
        {
            if (presetDefsInt == null)
            {
                presetDefsInt = new List<CustomRecipePresetDef>();
                foreach (var ratioTag in presetTags)
                {
                    var list = CustomNetworkRecipeReferences.TryGetPresetsOfTag(ratioTag);
                    if (list == null) continue;
                    presetDefsInt.AddRange(list);
                }
            }

            return presetDefsInt;
        }
    }
}