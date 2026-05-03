using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml;
using UnityEngine;
using Verse;

namespace TiberiumRim;

public class TiberiumTerrainDef : TerrainDef
{
    public List<Color> colorSpectrum = new();
    public float daysToNaturalCleanse = 20f;
    public TerrainDef decrystallized;
    public bool isDead = false;
    public float plantChanceFactor = 1f;
    public List<ThingProbability> plantSupport = new();
    public List<TerrainSupport> terrainSupport = new();

    public TerrainSupport TerrainSupportFor(TerrainDef def)
    {
        return terrainSupport.Find(s => s.TerrainTag.SupportsDef(def));
    }

    public bool SupportsPlant(ThingDef plant)
    {
        return plantSupport.Any(p => p.thing == plant);
    }

    public ThingDef TiberiumPlantFor()
    {
        foreach (var ps in plantSupport)
            if (TRUtils.Chance(ps.probability))
                return ps.thing;
        return null;
    }
}

public class ThingProbability
{
    public float probability;
    public ThingDef thing;

    public void LoadDataFromXmlCustom(XmlNode xmlRoot)
    {
        var parts = Regex.Replace(xmlRoot.FirstChild.Value, @"\s", "").Split(',');
        DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "thing", parts[0]);
        probability = ParseHelper.ParseFloat(parts[1]);
    }
}