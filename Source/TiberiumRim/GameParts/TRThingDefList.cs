using RimWorld;
using TeleCore;
using Verse;

namespace TR
{
    [DefOf]
    [StaticConstructorOnStartup]
    public static class SubThingGroupDefOf
    {
        public static SubMenuGroupDef Common;
        public static SubMenuGroupDef Forgotten;
        public static SubMenuGroupDef GDI;
        public static SubMenuGroupDef Nod;
        public static SubMenuGroupDef Scrin;
        public static SubMenuGroupDef Tiberium;
    }

    [DefOf]
    [StaticConstructorOnStartup]
    public static class SubThingCategoryDefOf
    {
        public static SubMenuCategoryDef Structure;
        public static SubMenuCategoryDef Decoration;
        public static SubMenuCategoryDef Processing;
        public static SubMenuCategoryDef Defense;
        public static SubMenuCategoryDef Research;
        public static SubMenuCategoryDef Misc;
        public static SubMenuCategoryDef Producers;
        public static SubMenuCategoryDef Crystals;

    }
}
