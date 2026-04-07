using System.Collections.Generic;
using System.Linq;
using TR.Defs;
using Verse;

namespace TR.GameParts;

public static class TRThingDefList
{
    public static Dictionary<FactionDesignationDef, Dictionary<TRThingCategoryDef, List<TRThingDef>>> Categorized =
        new();

    public static List<FactionDesignationDef> FactionDesignations = new();
    public static List<TRThingDef> AllDefs = new();

    static TRThingDefList()
    {
        var list1 = DefDatabase<FactionDesignationDef>.AllDefs;
        var list2 = DefDatabase<TRThingCategoryDef>.AllDefs;
        for (var i = 0; i < list1.Count(); i++)
        {
            var des = list1.ElementAt(i);
            var dict = new Dictionary<TRThingCategoryDef, List<TRThingDef>>();
            for (var j = 0; j < list2.Count(); j++)
            {
                var cat = list2.ElementAt(j);
                dict.Add(cat, new List<TRThingDef>());
            }

            Categorized.Add(des, dict);
            FactionDesignations.Add(des);
        }
    }

    public static int TotalCount
    {
        get { return Categorized.Sum(k => k.Value.Sum(k2 => k2.Value.Count)); }
    }

    public static bool HasUnDiscovered(FactionDesignationDef faction)
    {
        return Categorized[faction].Any(d => HasUnDiscovered(faction, d.Key));
    }

    public static bool HasUnDiscovered(FactionDesignationDef faction, TRThingCategoryDef category)
    {
        return Enumerable.Any(Categorized[faction][category],
            d => !d.ConstructionOptionDiscovered && d.IsActive(out var s));
    }

    public static void Add(TRThingDef def)
    {
        AllDefs.Add(def);
        if (def.factionDesignation == null || def.TRCategory == null)
        {
            Log.Error("REEE YOU FORGOT DESIGNATION AT " + def.defName);
            def.factionDesignation = FactionDesignationDefOf.Common;
            def.TRCategory = TRCategoryDefOf.Misc;
            return;
        }

        if (!Categorized[def.factionDesignation][def.TRCategory].Contains(def))
            Categorized[def.factionDesignation][def.TRCategory].Add(def);
        //if (!def.menuHidden)
        //Log.Error(def +  " should have menuHidden");
    }
}

[DefOf]
[StaticConstructorOnStartup]
public static class FactionDesignationDefOf
{
    public static FactionDesignationDef Common;
    public static FactionDesignationDef Forgotten;
    public static FactionDesignationDef GDI;
    public static FactionDesignationDef Nod;
    public static FactionDesignationDef Scrin;
    public static FactionDesignationDef Tiberium;
}

[DefOf]
[StaticConstructorOnStartup]
public static class TRCategoryDefOf
{
    public static TRThingCategoryDef Structure;
    public static TRThingCategoryDef Decoration;
    public static TRThingCategoryDef Processing;
    public static TRThingCategoryDef Defense;
    public static TRThingCategoryDef Research;
    public static TRThingCategoryDef Misc;
    public static TRThingCategoryDef Producers;
    public static TRThingCategoryDef Crystals;
}