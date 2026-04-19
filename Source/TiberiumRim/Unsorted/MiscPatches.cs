using HarmonyLib;
using RimWorld;
using Verse;

namespace TR;

public static class MiscPatches
{
    //Patching Pref Changes
    [HarmonyPatch(typeof(Prefs))]
    [HarmonyPatch(nameof(Prefs.BackgroundImageExpansion), MethodType.Setter)]
    public static class Prefs_BackgroundImageExpansionSetterPatch
    {
        public static void Postfix(ExpansionDef value)
        {
            if (value != null) TiberiumRimMod.CoreSettings.UseCustomBackground = false;
        }
    }
}