using System.Collections.Generic;
using RimWorld;
using TeleCore.UI;
using Verse;

namespace TeleCore.Types.Utils;

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