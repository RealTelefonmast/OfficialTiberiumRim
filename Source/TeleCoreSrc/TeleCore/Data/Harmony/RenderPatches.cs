using HarmonyLib;
using RimWorld;

namespace TeleCore.Harmony;

internal static class RenderPatches
{
    [HarmonyPatch(typeof(WeatherManager))]
    [HarmonyPatch(nameof(WeatherManager.DrawAllWeather))]
    public static class DrawAllWeatherPatch
    {
        public static void Postfix(Verse.Map ___map)
        {
            //TODO: Fixup renderer
            //___map.GetMapInfo<AtmosphericMapInfo>().DrawSkyOverlays();
        }
    }
}