using System.Collections.Generic;
using Verse;

namespace TeleCore.Rendering.UI.SpecialSubMenu;

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