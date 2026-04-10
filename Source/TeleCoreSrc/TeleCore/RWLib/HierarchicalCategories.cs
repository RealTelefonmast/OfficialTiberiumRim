using System.Collections.Generic;
using TeleCore.RWLib.XML;
using TeleCore.Shared;

namespace TeleCore.RWLib;

public static class HierarchicalCategories
{
    private static Dictionary<HierarchicalCategoryDef, IList<HierarchicalCategoryDef>> _hierarchyMap;
    private static Dictionary<HierarchicalCategoryDef, IList<HierarchicalCategoryDef>> _kindMap;

    static HierarchicalCategories()
    {
        _hierarchyMap = new Dictionary<HierarchicalCategoryDef, IList<HierarchicalCategoryDef>>();
        _kindMap = new Dictionary<HierarchicalCategoryDef, IList<HierarchicalCategoryDef>>();
    }

    #region Access

    public static IList<HierarchicalCategoryDef> ChildrenOf(HierarchicalCategoryDef def)
    {
        
        if (!_hierarchyMap.TryGetValue(def, out var list)) 
            return EmptyList<HierarchicalCategoryDef>.Get();
        return list;
    }

    public static IList<HierarchicalCategoryDef> HierarchiesInCategory(HierarchicalCategoryDef def)
    {
        if (!_kindMap.TryGetValue(def, out var list))
            return EmptyList<HierarchicalCategoryDef>.Get();
        return list;
    }

    #endregion

    internal static void Register(HierarchicalCategoryDef def)
    {
        RegisterInHierarchy(def);
    }

    private static void RegisterInHierarchy(HierarchicalCategoryDef def)
    {
        if (def.parent == null) return;
        if (!_hierarchyMap.TryGetValue(def.parent, out var list))
        {
            list = _hierarchyMap[def] = new List<HierarchicalCategoryDef>();
        }
        list.Add(def);
    }

    private static void RegisterInKind(HierarchicalCategoryDef def)
    {
        if (def.categoryKind == null) return;
        if (!_kindMap.TryGetValue(def.categoryKind, out var list))
        {
            list = _kindMap[def] = new List<HierarchicalCategoryDef>();
        }
        list.Add(def);
    }
}