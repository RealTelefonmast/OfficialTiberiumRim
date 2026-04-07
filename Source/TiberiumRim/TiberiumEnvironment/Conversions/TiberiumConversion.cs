using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml;
using TR.Defs;
using TR.GameParts;
using TR.TiberiumObjects;
using Verse;

namespace TR.TiberiumEnvironment.Conversions;

public class TiberiumConversion
{
    [NoTranslate] private string fromTerrain;

    public bool isTopLayer;
    public List<DefFloat<TiberiumCrystalDef>> toCrystal = new();
    public TerrainDef toTerrain;

    public TerrainDef FromTerrain => DefDatabase<TerrainDef>.GetNamedSilentFail(fromTerrain);
    public TerrainFilterDef FromTerrainGroup => DefDatabase<TerrainFilterDef>.GetNamedSilentFail(fromTerrain);

    public void GetOutcomes(out TiberiumCrystalDef crystalDef, out TerrainDef terrainDef, out bool isTopLayer)
    {
        crystalDef = toCrystal.RandomElementByWeight(t => t.value).def;
        terrainDef = toTerrain;
        isTopLayer = this.isTopLayer;
    }

    public bool HasOutcomesFor(TerrainDef def)
    {
        if (def == FromTerrain) return true;
        return FromTerrainGroup?.Allows(def) ?? false;
    }


    // TerrainDef -> TerrainDef , TiberiumCrystalDef1 : weight | TiberiumCrystalDef2 : weight |...
    public void LoadDataFromXmlCustom(XmlNode xmlRoot)
    {
        var arr1 = Regex.Replace(xmlRoot.FirstChild.Value, @"\s+", "")
            .Split(new[] { "->" }, StringSplitOptions.None); //Split(',');
        var arr2 = arr1[1].Split(',');
        var arr3 = arr2[1].Split('|');

        fromTerrain = arr1[0];

        //Get Terrain Outcome
        var terrainArr = arr2[0].Split(':');
        DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "toTerrain", terrainArr[0]);
        if (terrainArr.Length > 1)
            isTopLayer = ParseHelper.ParseBool(terrainArr[1]);

        //Get Crystal Outcomes
        foreach (var s in arr3)
        {
            var parts = s.Split(':');
            var val = parts.Length > 1 ? ParseHelper.ParseFloat(parts[1]) : 1f;
            toCrystal.Add(new DefFloat<TiberiumCrystalDef>(parts[0], val));
        }
    }
}