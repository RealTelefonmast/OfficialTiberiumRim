using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using TeleCore.Static;
using TeleCore.Utility;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace TeleCore.Harmony;

public static class UIPatches
{
    [HarmonyPatch(typeof(MouseoverReadout))]
    [HarmonyPatch(nameof(MouseoverReadout.MouseoverReadoutOnGUI))]
    static class MouseoverReadout_Patch
    {
        private static MethodBase ReadoutCall = AccessTools.Method(typeof(MouseoverReadout_Patch), nameof(DrawGasReadout));
        
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            var iList = codeInstructions.ToList();
            for (int i = 0; i < iList.Count; i++)
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

        static void DrawGasReadout(ref float curYOffset)
        {
            IntVec3 intVec = UI.MouseCell();
            var gasGrid =  Find.CurrentMap.GetMapInfo<SpreadingGasGrid>();
            if (gasGrid.AnyGasAtUnsafe(intVec))
            {
                var allGasses = gasGrid.CellStackAtUnsafe(UI.MouseCell().Index(gasGrid.Map));
                for (var i = 0; i < allGasses.Length; i++)
                {
                    var gasCell = allGasses[i];
                    if (gasCell.value >= 0)
                    {
                        var def = (SpreadingGasTypeDef)gasCell.defID;
                        Widgets.Label(
                            new Rect(MouseoverReadout.BotLeft.x,
                                (float)UI.screenHeight - MouseoverReadout.BotLeft.y - curYOffset, 999f, 999f),
                            $"{def}: ({gasCell.value}) ({gasCell.overflow}) ({gasCell.value / (float)def.maxDensityPerCell})");//[{allGasses[def].TotalGasCount}][{allGasses[def].TotalValue}]");
                        curYOffset += 19f;
                    }
                }
            }
        }
    }

    /*
    [HarmonyPatch(typeof(CellInspectorDrawer), "DrawMapInspector")]
    public static class DrawMapInspectorPatch
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            CellInspectorDrawer.DrawRow("TEST LABEL", "100% Data");

            var gasGrid =  Find.CurrentMap.GetMapInfo<SpreadingGasGrid>();
            var allGasses = gasGrid.AllGassesAt(UI.MouseCell());
            foreach (var gasCell in allGasses)
            {
                CellInspectorDrawer.DrawRow(gasCell.gasType.LabelCap, ((gasCell.density/(float)gasCell.gasType.maxDensityPerCell).ToStringPercent("F0") + $"[{gasGrid.Layers[gasCell.gasType.IDReference].TotalGasCount}]"));
            }
        }
    }
    */
    
    // TODO: Restore PlaySettingsPatch once atmosphere settings system is wired up (AtmosphereMod removed).
}