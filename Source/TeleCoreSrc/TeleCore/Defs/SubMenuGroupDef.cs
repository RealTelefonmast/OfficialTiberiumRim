using System.Collections.Generic;
using TeleCore.Types;
using Verse;

namespace TeleCore.Defs;

public class SubMenuGroupDef : Def
{
    public string groupIconPath;
    public bool isDevGroup = false;
    public SubBuildMenuDef parentDef;
    public List<SubMenuCategoryDef> subCategories;

    //Pack Def | BuildMenu | Des | Des_Sel | Tab | Tab_Sel
    public string subPackPath;

    [field: Unsaved] public DesignationTexturePack? TexturePack { get; private set; }

    public override void ResolveReferences()
    {
        base.ResolveReferences();
        if (parentDef != null)
        {
            if (parentDef.subMenus.Contains(this))
                return;
            parentDef.subMenus.Add(this);
        }
    }

    public override void PostLoad()
    {
        base.PostLoad();
        LongEventHandler.ExecuteWhenFinished(
            delegate { TexturePack ??= new DesignationTexturePack(subPackPath, this); });
    }
}