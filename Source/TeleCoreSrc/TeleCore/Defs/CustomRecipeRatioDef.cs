using System.Collections.Generic;
using TeleCore.Unsorted;
using Verse;

namespace TeleCore.Defs;

public class CustomRecipeRatioDef : Def
{
    public List<DefValueLoadable<NetworkValueDef, float>> byProducts;
    public bool hidden = false;
    public List<DefValueLoadable<NetworkValueDef, float>> inputRatio;
    public ThingDef result;
    public List<string> tags;

    public override void ResolveReferences()
    {
        base.ResolveReferences();

        //
        CustomNetworkRecipeReferences.TryRegister(this);
    }
}