using System.Collections.Generic;
using TeleCore.Defs.Values;
using TeleCore.Network.Flow.Values;
using TeleCore.Static;
using Verse;

namespace TeleCore.Defs;

public class CustomRecipeRatioDef : Def
{
    public List<DefValueLoadable<NetworkValueDef, float>> byProducts;
    public List<DefValueLoadable<NetworkValueDef, float>> inputRatio;
    public bool hidden = false;
    public ThingDef result;
    public List<string> tags;

    public override void ResolveReferences()
    {
        base.ResolveReferences();

        //
        CustomNetworkRecipeReferences.TryRegister(this);
    }
}