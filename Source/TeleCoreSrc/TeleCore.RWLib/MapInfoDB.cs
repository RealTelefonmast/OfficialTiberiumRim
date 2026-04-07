using System;
using System.Collections.Generic;
using Verse;

namespace TeleCore.RWLib;

public static class MapInfoDB
{
    private static readonly Dictionary<string, Type> TypeByName = new ();

    internal static void Init()
    {
        var allInfos = typeof(MapInformation).AllSubclassesNonAbstract();
        foreach (var info in allInfos)
        {
            var attribute = info.TryGetAttribute<MapInfoAttribute>();
            if (attribute != null)
            {
                TypeByName[attribute.Name] = info;
            }
        }
    }

    public static Type GetTypeByName(string name)
    {
        if(TypeByName.TryGetValue(name, out var type))
            return type;
        throw new ArgumentException($"Could not find a MapInformation with the name {name}");
    }
}