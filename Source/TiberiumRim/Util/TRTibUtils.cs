using System;
using System.Collections.Generic;
using RimWorld;
using TR.Components;
using TR.WorldInfos;
using Verse;
using WorldComponent_TR = TR.World.WorldComponent_TR;

namespace TR;

public static class TRTibUtils
{
    /// Incident Locks
    internal static bool AllowTRInit(this WorldComponent_TR tr)
    {
        return Tiberium().GetWorldInfo<GroundZeroInfo>().HasGroundZero;
    }

    internal static bool AllowNewMeteorites(this WorldComponent_TR tr)
    {
        return TiberiumDefOf.MineralAnalysis.IsFinished;
    }

    public static MapComponent_Tiberium Tiberium(this Map map)
    {
        if (map == null)
        {
            TRLog.Warning("Map is null for Tiberium MapComp getter");
            return null;
        }

        return StaticData.TiberiumMapComp[map.uniqueID];
    }

    public static WorldComponent_TR Tiberium()
    {
        return Find.World.GetComponent<WorldComponent_TR>();
    }

    public static TRGameSettingsInfo GameSettings()
    {
        return Tiberium().GameSettings;
    }

    public static ThingDef GetWrappedCorpseDef(this Corpse corpse)
    {
        var thingDef = corpse.def;
        var d = new ThingDef();
        d.category = ThingCategory.Item;
        d.thingClass = typeof(WrappedCorpse);
        d.selectable = true;
        d.tickerType = TickerType.Normal;
        d.altitudeLayer = AltitudeLayer.ItemImportant;
        d.scatterableOnMapGen = false;
        d.SetStatBaseValue(StatDefOf.Beauty, -50f);
        d.SetStatBaseValue(StatDefOf.DeteriorationRate, 1f);
        d.SetStatBaseValue(StatDefOf.FoodPoisonChanceFixedHuman, 0.05f);
        d.alwaysHaulable = true;
        d.soundDrop = SoundDefOf.Corpse_Drop;
        d.pathCost = DefGenerator.StandardItemPathCost;
        d.socialPropernessMatters = false;
        d.tradeability = Tradeability.None;
        d.messageOnDeteriorateInStorage = false;
        d.inspectorTabs = new List<Type>(thingDef.inspectorTabs);
        d.modContentPack = thingDef.modContentPack;
        d.ingestible = thingDef.ingestible;
        d.comps.AddRange(thingDef.comps);
        d.comps.Add(new CompProperties_Forbiddable());
        d.recipes = new List<RecipeDef>(thingDef.recipes);
        d.defName = "Wrapped_" + corpse.def.defName;
        d.label = thingDef.label;
        d.description = thingDef.description;
        d.soundImpactDefault = thingDef.soundImpactDefault;
        d.SetStatBaseValue(StatDefOf.MarketValue, thingDef.GetStatValueAbstract(StatDefOf.MarketValue));
        d.SetStatBaseValue(StatDefOf.Flammability, thingDef.GetStatValueAbstract(StatDefOf.Flammability));
        d.SetStatBaseValue(StatDefOf.MaxHitPoints, thingDef.GetStatValueAbstract(StatDefOf.MaxHitPoints));
        d.SetStatBaseValue(StatDefOf.Mass, thingDef.GetStatValueAbstract(StatDefOf.Mass));
        d.SetStatBaseValue(StatDefOf.Nutrition, thingDef.GetStatValueAbstract(StatDefOf.Nutrition));
        d.thingCategories = new List<ThingCategoryDef>(thingDef.thingCategories);
        return d;
    }
}