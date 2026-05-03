using System.Collections.Generic;
using RimWorld;
using Verse;

namespace TeleCore.Unsorted;

internal static class IncidentFilterDB
{
    public static Dictionary<IncidentDef, AtmosphericIncidentFilter> filtersByIncident;

    public static bool Inject_CanFireNow(IncidentDef def, Map map)
    {
        if (filtersByIncident.TryGetValue(def, out var value))
        {
            var volume = map.GetMapInfo<AtmosphericMapInfo>().MapVolume;
            return volume.StoredPercentOf(value.AtmosValueDef) >= value.threshold;
        }

        return true;
    }
}