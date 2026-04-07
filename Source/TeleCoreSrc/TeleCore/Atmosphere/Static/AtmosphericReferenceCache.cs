using System.Collections.Generic;
using TeleCore.Atmosphere.Defs;

namespace TeleCore.Atmosphere.Static;

public static class AtmosphericReferenceCache
{
    private static readonly List<AtmosphericValueDef> EmptyList = new();
    public static readonly Dictionary<string, List<AtmosphericValueDef>> AtmosphericGroupsByTag;

    static AtmosphericReferenceCache()
    {
        AtmosphericGroupsByTag = new Dictionary<string, List<AtmosphericValueDef>>();
    }

    public static List<AtmosphericValueDef> AtmospheresOfTag(string tag)
    {
        if (tag != null && AtmosphericGroupsByTag.TryGetValue(tag, out var group)) return group;
        return EmptyList;
    }

    public static void RegisterDef(AtmosphericValueDef valueDef)
    {
        if (valueDef.atmosphericTag == null) return;
        if (!AtmosphericGroupsByTag.TryGetValue(valueDef.atmosphericTag, out var groupList))
        {
            groupList = new List<AtmosphericValueDef>();
            AtmosphericGroupsByTag.Add(valueDef.atmosphericTag, groupList);
        }

        //
        groupList.Add(valueDef);
    }
}