using HarmonyLib;
using RimWorld;

namespace TeleCore.Patches
{
    internal static class RenderPatches
    {
        [HarmonyPatch(typeof(WeatherManager))]
        [HarmonyPatch(nameof(WeatherManager.DrawAllWeather))]
        public static class DrawAllWeatherPatch
        {
            public static void Postfix(Verse.Map ___map)
            {
                ___map.GetMapInfo<AtmosphericMapInfo>().DrawSkyOverlays();
            }
        }
    }
}
