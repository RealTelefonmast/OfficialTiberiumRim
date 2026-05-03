using System;
using TiberiumRim;
using TR.Networks.AtmosphericNetwork;
using UnityEngine;
using Verse;

namespace TR;

public static class TibUtils
{
    public static bool IsTiberiumTerrain(this TerrainDef def)
    {
        return def is TiberiumTerrainDef;
    }

    public static TiberiumCrystalDef CrystalDefFromType(NetworkValueDef valueDef, out bool isGas)
    {
        isGas = false;
        //TODO: Adjust drops
        if (valueDef == TiberiumDefOf.TibGreen) return TiberiumDefOf.TiberiumGreen;

        if (valueDef == TiberiumDefOf.TibBlue) return TiberiumDefOf.TiberiumBlue;

        if (valueDef == TiberiumDefOf.TibRed) return TiberiumDefOf.TiberiumRed;

        if (valueDef == TiberiumDefOf.TibSludge) return TiberiumDefOf.TiberiumMossGreen;

        if (valueDef == TiberiumDefOf.TibGas) isGas = true;

        return null;
    }


    public static Texture2D MaterialForType(TiberiumValueType valueType)
    {
        var tex = SolidColorMaterials.NewSolidColorTexture(Color.black);
        var def = MainTCD.Main;
        switch (valueType)
        {
            case TiberiumValueType.Green:
                tex = TRMats.GreenType;
                break;
            case TiberiumValueType.Blue:
                tex = TRMats.BlueType;
                break;
            case TiberiumValueType.Red:
                tex = TRMats.RedType;
                break;
            case TiberiumValueType.Sludge:
                tex = TRMats.SludgeType;
                break;
            case TiberiumValueType.Gas:
                tex = TRMats.GasType;
                break;
        }

        return tex;
    }

    public static Color GetColor(this Enum valueType)
    {
        var color = Color.white;
        if (valueType is TiberiumValueType tibType)
        {
            var def = MainTCD.Main;
            color = tibType switch
            {
                TiberiumValueType.Green => def.GreenColor,
                TiberiumValueType.Blue => def.BlueColor,
                TiberiumValueType.Red => def.RedColor,
                TiberiumValueType.Sludge => def.SludgeColor,
                TiberiumValueType.Gas => def.GasColor,
                _ => color
            };
        }

        return color;
    }

    public static string ShortLabel(this Enum enumType)
    {
        if (enumType is TiberiumValueType tibVal) return ShortLabel(tibVal);

        if (enumType is AtmosphericValueType atmosVal) return ShortLabel(atmosVal);

        return string.Empty;
    }

    public static string ShortLabel(this AtmosphericValueType valueType)
    {
        var label = valueType switch
        {
            AtmosphericValueType.Air => "Air",
            AtmosphericValueType.Pollution => "Poll",
            _ => string.Empty
        };

        return label;
    }

    public static string ShortLabel(this TiberiumValueType valueType)
    {
        var label = valueType switch
        {
            TiberiumValueType.Green => "G",
            TiberiumValueType.Blue => "B",
            TiberiumValueType.Red => "R",
            TiberiumValueType.Sludge => "Slg",
            TiberiumValueType.Gas => "Gs",
            _ => string.Empty
        };

        return label;
    }

    public static class Network
    {
        public static NetworkValueDef[] MainValueTypes = new[]
            { TiberiumDefOf.TibGreen, TiberiumDefOf.TibBlue, TiberiumDefOf.TibRed };
    }
}