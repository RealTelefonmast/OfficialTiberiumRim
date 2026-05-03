using System;
using System.Collections.Generic;
using TeleCore.Unsorted;
using Verse;

namespace TeleCore.Defs;

public class SubBuildMenuDef : Def
{
    //
    public List<SubMenuGroupDef>? subMenus;
    public string superPackPath;
    public Type visWorker = typeof(SubMenuVisibilityWorker);

    //
    [field: Unsaved] public DesignationTexturePack TexturePack { get; private set; }

    [field: Unsaved] public SubMenuVisibilityWorker VisWorker { get; private set; }

    public override void ResolveReferences()
    {
        base.ResolveReferences();
        if (subMenus == null)
        {
            subMenus = new List<SubMenuGroupDef>();
            return;
        }

        foreach (var subMenu in subMenus) subMenu.parentDef = this;
    }

    public override void PostLoad()
    {
        base.PostLoad();
        LongEventHandler.ExecuteWhenFinished(delegate
        {
            TexturePack ??= new DesignationTexturePack(superPackPath, this);
            VisWorker = (SubMenuVisibilityWorker)Activator.CreateInstance(visWorker);
        });
    }
}