using System.Collections.Generic;
using System.Linq;
using TeleCore.Unsorted;
using Verse;

namespace TeleCore.Defs;

public class CustomRecipePresetDef : Def
{
    public List<DefValueLoadable<CustomRecipeRatioDef, int>> desiredResources;

    public List<string> tags;

    [field: Unsaved] public List<ThingDefCount> Results { get; private set; }

    [field: Unsaved] public string CostLabel { get; private set; }

    public bool HasByProducts => desiredResources.Any(r => r.Def.byProducts != null);

    public override void ResolveReferences()
    {
        base.ResolveReferences();
        Results = desiredResources.Select(m => new ThingDefCount(m.Def.result, m.Value)).ToList();
        CostLabel = NetworkBillUtility.CostLabel(NetworkBillUtility.ConstructCustomCostStack(desiredResources));

        //
        CustomNetworkRecipeReferences.TryRegister(this);
    }
}