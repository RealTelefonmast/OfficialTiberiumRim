using System;
using System.Collections.Generic;
using TeleCore.Types;
using TeleCore.Types.Utils;
using TeleCore.UI;
using Verse;

namespace TeleCore.Defs;

public class TeleDefExtension : DefModExtension
{
    //
    private AlternateGraphicWorker _altGraphicWorkerInt;

    //
    public bool addCustomTick;

    public DiscoveryProperties discovery;
    public List<GraphicData> extraGraphics;
    public Type graphicAlternateWorkerType;
    public ProjectileDefExtension projectile;

    //
    public ThingGroupCollection thingGroups = new();

    public AlternateGraphicWorker AlternateGraphicWorker
    {
        get
        {
            return _altGraphicWorkerInt ??=
                (AlternateGraphicWorker)Activator.CreateInstance(graphicAlternateWorkerType);
        }
    }
}

//
public class ThingGroupCollection
{
    public readonly List<ThingGroupDef> groups = new() { ThingGroupDefOf.All };

    /*TODO: Add listed items to list
    public void LoadDataFromXmlCustom(XmlNode xmlRoot)
    {
        if (xmlRoot.FirstChild?.Name == "<li>")
        {
            return;
        }
        var innerValue = xmlRoot.InnerText;
        string s = Regex.Replace(innerValue, @"\s+", "");
        string[] array = s.Split('|');
        for (var i = 0; i < array.Length; i++)
        {
            var groupDefname = array[i];
        }
    }
    */
}