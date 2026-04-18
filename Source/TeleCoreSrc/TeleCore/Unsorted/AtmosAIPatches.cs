using HarmonyLib;
using TeleCore.Comps;
using Verse;
using Verse.AI;

namespace TeleCore.Unsorted;

internal static class AtmosAIPatches
{
    [HarmonyPatch(typeof(Pawn_PathFollower))]
    [HarmonyPatch(nameof(Pawn_PathFollower.TrySetNewPath))]
    public static class PathFollower_TrySetNewPathPatch
    {
        //Patch the new path setter method to hook into new logic depending on the path
        public static void Postfix(Pawn_PathFollower __instance, ref bool __result, Pawn ___pawn)
        {
            //Bad path... return
            if (!__result) return;
            ___pawn.GetComp<Comp_PathFollowerExtra>().Notify_NewPath(__instance.curPath);
        }
    }

    [HarmonyPatch(typeof(Pawn_PathFollower))]
    [HarmonyPatch(nameof(Pawn_PathFollower.StopDead))]
    public static class PathFollower_StopDeadPatch
    {
        //
        public static void Postfix(Pawn ___pawn)
        {
            ___pawn.GetComp<Comp_PathFollowerExtra>().Notify_StopDead();
        }
    }

    [HarmonyPatch(typeof(Pawn_PathFollower))]
    [HarmonyPatch(nameof(Pawn_PathFollower.SetupMoveIntoNextCell))]
    public static class PathFollower_SetupMoveIntoNextCell
    {
        //
        public static bool Prefix(Pawn_PathFollower __instance, Pawn ___pawn, IntVec3 ___nextCell)
        {
            if (!___pawn.GetComp<Comp_PathFollowerExtra>().CanSetupMoveIntoNextCell(___nextCell))
                return false;
            return true;
        }
    }

    [HarmonyPatch(typeof(Pawn_PathFollower))]
    [HarmonyPatch(nameof(Pawn_PathFollower.TryEnterNextPathCell))]
    public static class TryEnterNextPathCellPatch
    {
        public static bool Prefix(Pawn_PathFollower __instance, Pawn ___pawn, IntVec3 ___nextCell)
        {
            if (!___pawn.GetComp<Comp_PathFollowerExtra>().CanEnterNextCell(___nextCell)) return false;
            return true;
        }
    }
}
