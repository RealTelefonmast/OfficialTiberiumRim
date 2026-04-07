using System.Collections.Generic;
using System.Linq;
using RimWorld;
using TeleCore.Logging;
using TeleCore.Utils;
using Verse;

namespace TeleCore.BuildMenu;

internal static class SubMenuThingDefList
{
    public static Dictionary<SubMenuGroupDef, Dictionary<SubMenuCategoryDef, List<BuildableDef>>> Categorized = new();
    public static List<SubMenuGroupDef> AllSubGroupDefs = new();


    public static Dictionary<SubMenuGroupDef, Dictionary<SubMenuCategoryDef, List<Designator>>> ResolvedDesignators =
        new();

    static SubMenuThingDefList()
    {
        var list1 = DefDatabase<SubMenuGroupDef>.AllDefs;
        var list2 = DefDatabase<SubMenuCategoryDef>.AllDefs;
        for (var i = 0; i < list1.Count(); i++)
        {
            var des = list1.ElementAt(i);
            var dict = new Dictionary<SubMenuCategoryDef, List<BuildableDef>>();
            var designatorDict = new Dictionary<SubMenuCategoryDef, List<Designator>>();
            for (var j = 0; j < list2.Count(); j++)
            {
                var cat = list2.ElementAt(j);
                dict.Add(cat, new List<BuildableDef>());
                designatorDict.Add(cat, new List<Designator>());
            }

            Categorized.Add(des, dict);
            AllSubGroupDefs.Add(des);
            ResolvedDesignators.Add(des, designatorDict);
        }
    }

    public static bool IsActive(SubMenuGroupDef groupDef)
    {
        var dict = Categorized[groupDef];
        return dict.Values.SelectMany(buildings => buildings).Any(def => IsActive(groupDef.parentDef, def));
    }

    //Discovery
    public static bool IsActive(SubBuildMenuDef inMenu, BuildableDef def)
    {
        return DebugSettings.godMode || (inMenu.VisWorker?.IsAllowed(def) ?? true);
    }

    public static void Add(BuildableDef def, BuildMenuExtension extension)
    {
        var groupDef = extension.groupDef;
        var category = extension.category;
        if (groupDef == null || category == null)
        {
            TLog.Error($"Error at {def} in 'subMenuDesignation': [{groupDef}, {category}, {extension.isDevOption}] {(groupDef == null ? "groupDef missing." : "")} {(category == null ? "category missing." : "")}");
            return;
        }

        if (!Categorized[groupDef][category].Contains(def))
        {
            Categorized[groupDef][category].Add(def);
            ResolvedDesignators[groupDef][category].Add(new Designator_Build(def));
        }
    }
}