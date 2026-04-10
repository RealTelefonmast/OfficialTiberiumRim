using HarmonyLib;
using TeleCore.Events;
using TeleCore.Events.Args;
using Verse;

namespace TeleCore.Mod.Patches;

internal static class GridPatches
{
    [HarmonyPatch(typeof(TerrainGrid), nameof(TerrainGrid.SetTerrain))]
    private static class TerrainGridSetTerrainPatch
    {
        private static void Postfix(TerrainGrid __instance, IntVec3 c, TerrainDef newTerr)
        {
            var previous = __instance.topGrid[__instance.map.cellIndices.CellToIndex(c)];
            GlobalEventHandler.OnTerrainChanged(new TerrainChangedEventArgs(c, false, previous, newTerr));
        }
    }

    [HarmonyPatch(typeof(TerrainGrid), nameof(TerrainGrid.SetUnderTerrain))]
    private static class TerrainGridSetUnderTerrainPatch
    {
        private static void Postfix(TerrainGrid __instance, IntVec3 c, TerrainDef newTerr)
        {
            var previous = __instance.underGrid[__instance.map.cellIndices.CellToIndex(c)];
            GlobalEventHandler.OnTerrainChanged(new TerrainChangedEventArgs(c, true, previous, newTerr));
        }
    }
}