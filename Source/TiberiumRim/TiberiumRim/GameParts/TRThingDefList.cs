using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace TiberiumRim
{
    public static class TRThingDefList
    {
        public static Dictionary<FactionDesignationDef, Dictionary<TRThingCategoryDef, List<TRThingDef>>> Categorized = new Dictionary<FactionDesignationDef, Dictionary<TRThingCategoryDef, List<TRThingDef>>>();
        public static List<FactionDesignationDef> FactionDesignations = new List<FactionDesignationDef>();
        public static List<TRThingDef> AllDefs = new List<TRThingDef>();

        static TRThingDefList()
        {
            var list1 = DefDatabase<FactionDesignationDef>.AllDefs;
            var list2 = DefDatabase<TRThingCategoryDef>.AllDefs;
            for (int i = 0; i < list1.Count(); i++)
            {
                FactionDesignationDef des = list1.ElementAt(i);
                var dict = new Dictionary<TRThingCategoryDef, List<TRThingDef>>();
                for (int j = 0; j < list2.Count(); j++)
                {
                    TRThingCategoryDef cat = list2.ElementAt(j);
                    dict.Add(cat, new List<TRThingDef>());
                }
                Categorized.Add(des, dict);
                FactionDesignations.Add(des);
            }
        }

        public static int TotalCount
        {
            get
            {
                return Categorized.Sum(k => k.Value.Sum(k2 => k2.Value.Count));
            }
        }

        public static void Add(TRThingDef def)
        {
            if (def.hidden) return;
            AllDefs.Add(def);
            if (def.factionDesignation == null || def.TRCategory == null)
            {
                Log.Error("REEE YOU FORGOT DESIGNATION AT " + def.defName);
                def.factionDesignation = FactionDesignationDefOf.Common;
                def.TRCategory = TRCategoryDefOf.Misc;
                return;
            }
            if (!Categorized[def.factionDesignation][def.TRCategory].Contains(def))
            {
                Categorized[def.factionDesignation][def.TRCategory].Add(def);
            }
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
}
