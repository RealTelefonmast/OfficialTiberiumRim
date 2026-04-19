using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using TR.SuperWeapon;
using UnityEngine;

namespace TR;

public static class AsatPatches
{
    [HarmonyPatch(typeof(WorldSelector))]
    [HarmonyPatch("HandleWorldClicks")]
    public static class HandleWorldClicksPatch
    {
        public static bool Prefix(WorldSelector __instance)
        {
            if (Event.current.type == EventType.MouseDown)
                if (Event.current.button == 1 && __instance.NumSelectedObjects > 0)
                {
                    var obj = __instance.FirstSelectedObject;
                    if (obj is AttackSatellite asat)
                    {
                        asat.SetDestination(GenWorld.MouseTile());
                        Event.current.Use();
                        return false;
                    }
                }

            return true;
        }
    }
}