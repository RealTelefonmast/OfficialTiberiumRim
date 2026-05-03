using System.Collections.Generic;
using TeleCore.Defs;
using TeleCore.Designators;
using Verse;

namespace TeleCore.Unsorted;

public class SubMenuDesignationCategoryDef : DesignationCategoryDef
{
    public bool isDebug;
    public SubBuildMenuDef menuDef;

    public override void ResolveReferences()
    {
        base.ResolveReferences();
        LongEventHandler.ExecuteWhenFinished(() =>
        {
            resolvedDesignators.Clear();
            resolvedDesignators ??= new List<Designator>();
            resolvedDesignators.Add(new Designator_SubBuildMenu(menuDef));
        });
    }
}