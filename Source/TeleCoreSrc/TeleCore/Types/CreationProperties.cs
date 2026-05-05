using System.Collections.Generic;
using System.Linq;
using System.Xml;
using RimWorld;
using Verse;

namespace TeleCore.Types;

public class CreationProperties
{
    public List<CreationOptionProperties> thingsToCreate;

    public int TotalCountToMake => thingsToCreate.Sum(t => t.amount);

    public string TargetLabel()
    {
        if (thingsToCreate.All(t => t.def.category == ThingCategory.Building))
            return "TR_TaskCreationsBuild".Translate();

        if (thingsToCreate.All(t => t.def.category == ThingCategory.Item))
            return "TR_TaskCreationsCraft".Translate();

        return "TR_TaskCreationsBoth".Translate();
    }
}

public class CreationOptionProperties
{
    public int amount = 1;
    public ThingDef def;
    public QualityCategory? quality;
    public ThingDef stuffDef;

    public bool Accepts(Thing thing)
    {
        return (quality == null || (thing.TryGetQuality(out var qc) && qc == quality)) &&
               (stuffDef == null || thing.Stuff == stuffDef);
    }

    public void LoadDataFromXmlCustom(XmlNode xmlRoot)
    {
        //TODO: Add additional reader options
        DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "def", xmlRoot.Name);
        amount = ParseHelper.FromString<int>(xmlRoot.FirstChild.Value);
    }

    public override string ToString()
    {
        return $"{def}[{stuffDef}&{quality}]: {amount}";
    }
}