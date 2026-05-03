using TeleCore.Unsorted;
using Verse;

namespace TeleCore.Defs;

public class HierarchicalCategoryDef : Def
{
    public HierarchicalCategoryDef categoryKind;
    public HierarchicalCategoryDef parent;

    public override void PostLoad()
    {
        base.PostLoad();
        HierarchicalCategories.Register(this);
    }
}