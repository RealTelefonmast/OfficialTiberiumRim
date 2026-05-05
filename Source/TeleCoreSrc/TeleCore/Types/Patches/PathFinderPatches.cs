using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using TeleCore.MapComponents;
using TeleCore.Types.Exposables;
using TeleCore.Types.Utils;
using Verse;
using Verse.AI;

namespace TeleCore.Types.Patches;

internal static class PathFinderPatches
{
    //Current Path-Find Request
    internal static SearchParams _currentSearch;
    internal static HashSet<AvoidGridWorker> _customAvoidGrids = new();

    internal static GenericPathFollower? UsedGenericPather { get; set; }

    internal static void ResolveDataFor(IntVec3 start, LocalTargetInfo dest, TraverseParms traverseParms, Map map)
    {
        _currentSearch = new SearchParams(start, dest, traverseParms);

        var thing = traverseParms.pawn ?? UsedGenericPather?.Thing;
        if (thing == null) return;

        //TODO: Clean request of set?
        foreach (var avoidGrid in map.GetMapInfo<PathHelperInfo>().AvoidGrids)
            if (avoidGrid.AffectsThing(thing))
                _customAvoidGrids.Add(avoidGrid);
    }

    internal static int AdjustWeight(int index)
    {
        var extraVal = 0;
        if (_customAvoidGrids.Count == 0) return 0;
        foreach (var avoidGrid in _customAvoidGrids)
            extraVal += avoidGrid.Grid[index];
        return extraVal;
    }

    internal struct SearchParams
    {
        internal IntVec3 start;
        internal LocalTargetInfo dest;
        internal TraverseParms traverseParms;

        internal bool IsValid => start.IsValid && dest.IsValid && traverseParms.pawn != null;

        internal SearchParams(IntVec3 start, LocalTargetInfo dest, TraverseParms traverseParms)
        {
            this.start = start;
            this.dest = dest;
            this.traverseParms = traverseParms;
        }

        internal static SearchParams Empty => new();
    }

    //

    #region Walkable

    [HarmonyPatch(typeof(GenGrid), nameof(GenGrid.WalkableBy))]
    internal static class GenGridWalkablyPatch
    {
        public static void Postfix(ref bool __result)
        {
        }
    }

    #endregion

    //Path Finder
    [HarmonyPatch(typeof(PathFinder))]
    [HarmonyPatch(nameof(PathFinder.FindPath), typeof(IntVec3), typeof(LocalTargetInfo), typeof(TraverseParms),
        typeof(PathEndMode), typeof(PathFinderCostTuning))]
    internal static class PathFinderFindPathPatch
    {
        internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var resolveData = AccessTools.Method(typeof(PathFinderPatches), nameof(ResolveDataFor));
            var adjustWeight = AccessTools.Method(typeof(PathFinderPatches), nameof(AdjustWeight));
            var mapField = AccessTools.Field(typeof(PathFinder), nameof(PathFinder.map));

            var failSafe = true;
            var failSafe2 = true;
            CodeInstruction previous = null;
            foreach (var instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Stloc_S && instruction.operand is LocalBuilder { LocalIndex: 13 })
                {
                    failSafe = false;
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Ldarg_2);
                    yield return new CodeInstruction(OpCodes.Ldarg_3);
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, mapField);
                    yield return new CodeInstruction(OpCodes.Call, resolveData);
                }

                //
                if (previous != null && previous.opcode == OpCodes.Stloc_S &&
                    previous.operand is LocalBuilder { LocalIndex: 49 })
                {
                    failSafe2 = false;
                    yield return new CodeInstruction(OpCodes.Ldloc_S, 45);
                    yield return new CodeInstruction(OpCodes.Call, adjustWeight);
                    yield return new CodeInstruction(OpCodes.Ldloc_S, 49);
                    yield return new CodeInstruction(OpCodes.Add);
                    yield return new CodeInstruction(OpCodes.Stloc_S, 49);
                }

                //
                previous = instruction;
                yield return instruction;
            }

            //
            if (failSafe || failSafe2)
                TLog.Error($"Failed to patch {nameof(PathFinderFindPathPatch)}!\n[0]: {failSafe} [1]: {failSafe2}");
        }

        internal static void Postfix(ref PawnPath __result)
        {
            if (__result.Found)
            {
                //
            }

            _currentSearch = SearchParams.Empty;
            _customAvoidGrids.Clear();
        }
    }

    //Path Costs For Region
    [HarmonyPatch(typeof(RegionCostCalculator))]
    [HarmonyPatch(nameof(RegionCostCalculator.GetCellCostFast))]
    internal static class RegionCostCalculatorGetCellCostFastPatch
    {
        public static void Postfix(RegionCostCalculator __instance, int index, ref int __result)
        {
            foreach (var avoidGrid in _customAvoidGrids) __result += avoidGrid.Grid[index];
        }
    }

    //

    //Hanlder Wandering

    #region Wandering

    [HarmonyPatch(typeof(RCellFinder), nameof(RCellFinder.CanWanderToCell))]
    internal static class RCellFinderCanWanderToCellPatch
    {
    }

    [HarmonyPatch(typeof(RCellFinder), nameof(RCellFinder.RandomWanderDestFor))]
    internal static class RCellFinderRandomWanderDestForPatch
    {
    }

    #endregion
}