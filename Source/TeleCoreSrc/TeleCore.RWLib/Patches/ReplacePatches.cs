using HarmonyLib;
using RimWorld;
using TeleCore.Loader;
using TeleCore.Network.Bills;
using TeleCore.Shared;

namespace TeleCore.Loading;

internal static class ReplacePatches
{
    [HarmonyPatch(typeof(BillUtility))]
    [HarmonyPatch("MakeNewBill")]
    public static class MakeNewBillPatch
    {
        public static void Postfix(ref Bill __result)
        {
            if (__result.recipe is RecipeDef_Network { networkCost.Valid: true } tRecipe)
            {
                var billProductionNetworkBill = new Bill_Production_Network(tRecipe);
                __result = billProductionNetworkBill;
            }
        }
    }

    //[HarmonyPatch(typeof(GenAttribute))]
    //[HarmonyPatch(nameof(GenAttribute.TryGetAttribute), typeof(MemberInfo), typeof(object))]
    public static class GenAttribute_Patch
    {
        public static bool Prefix()
        {
            TLog.Debug("Prefix'd");
            return true;
        }
    }
}