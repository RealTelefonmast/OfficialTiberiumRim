using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using TeleCore.MapComponents;
using UnityEngine;
using Verse;

namespace TeleCore.Unsorted;

public static class GasGridPatches
{
    [HarmonyPatch(typeof(MouseoverReadout))]
    [HarmonyPatch("MouseoverReadoutOnGUI")]
    private static class MouseoverReadout_Patch
    {
        private static readonly MethodBase ReadoutCall =
            AccessTools.Method(typeof(DrawUtils), nameof(DrawUtils.TryDrawExtraGasGrid));

        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            var iList = codeInstructions.ToList();
            for (var i = 0; i < iList.Count; i++)
            {
                var instruction = iList[i];

                if (i == iList.Count - 1)
                {
                    yield return new CodeInstruction(OpCodes.Ldloca_S, 1);
                    yield return new CodeInstruction(OpCodes.Call, ReadoutCall);
                }

                yield return instruction;
            }
        }
    }

    public static class DrawUtils
    {
        public static void TryDrawExtraGasGrid(ref float curYOffset)
        {
            var minimalGasGrid = Find.CurrentMap.GetComponent<GasGridSystem>();
            IntVec3 intVec = UI.MouseCell();
            if (minimalGasGrid.AnyGasAt(intVec))
            {
                Widgets.Label(
                    new Rect(MouseoverReadout.BotLeft.x,
                        (float)UI.screenHeight - MouseoverReadout.BotLeft.y - curYOffset, 999f, 999f),
                    "This is where you'd see gas if it were done!" + " " + (69 / 255f).ToStringPercent("F0"));
                curYOffset += 19f;
            }
        }
    }
}