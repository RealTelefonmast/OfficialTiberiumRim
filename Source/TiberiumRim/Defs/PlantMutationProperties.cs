using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using RimWorld;
using Verse;

namespace TiberiumRim;

public class PlantMutationGroup : Def
{
    public List<PlantChance> plantChances;
    public List<string> tags;

    public TRThingDef GetMutatedPlantFrom(Plant plant)
    {
        var nullChance = 1 - plantChances.Sum(e => e.chance);
        if (TRUtils.Chance(nullChance))
            return null;

        var rand = plantChances.InRandomOrder();
        for (var i = 0; i < rand.Count() - 1; i++)
        {
            var evolution = rand.ElementAt(i);
            if (TRUtils.Chance(evolution.chance)) return evolution.plant;
        }

        if (tags.Any(t => plant.def.defName.Contains(t)))
            foreach (var chance in plantChances)
                if (TRUtils.Chance(chance.chance))
                    return chance.plant;

        return plantChances.Last().plant;
    }
}

public class PlantChance
{
    public float chance;
    public TRThingDef plant;

    public void LoadDataFromXmlCustom(XmlNode xmlRoot)
    {
        DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "thing", xmlRoot.Name);
        var parts = Regex.Replace(xmlRoot.FirstChild.Value, @"\s", "").Split(',');
        chance = ParseHelper.FromString<float>(parts[0]);
    }
}

public class PlantGroupChance
{
    public float chance = 1f;
    public List<WeightedThing> plants = new();
}

public class WeightedThing
{
    public ThingDef thing;
    public float weight = 1;

    public void LoadDataFromXmlCustom(XmlNode xmlRoot)
    {
        var parts = Regex.Replace(xmlRoot.FirstChild.Value, @"\s", "").Split(',');
        DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "thing", parts[0]);
        if (parts.Length > 1)
            weight = ParseHelper.FromString<float>(parts[1]);
    }
}