using System;
using System.Collections.Generic;
using Verse;

namespace TeleCore.Unsorted;

public class TargetProperties
{
    public string groupLabel;
    public List<ThingDef> targetDefs;
    public Type targetType;

    public bool Accepts(Thing thing)
    {
        if (targetType != null && thing.def.thingClass == targetType) return true;
        if (targetDefs.NullOrEmpty()) return false;
        return targetDefs.Contains(thing.def);
    }
}