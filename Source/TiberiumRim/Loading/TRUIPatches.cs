using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public static class TRUIPatches
    {
        [HarmonyPatch(typeof(PlaySettings))]
        [HarmonyPatch(nameof(PlaySettings.DoPlaySettingsGlobalControls))]
        public static class PlaySettingsPatch
        {
            public static void Postfix(WidgetRow row, bool worldView)
            {
                if (worldView || row == null) return;

                row.ToggleableIcon(ref TRUtils.GameSettings().EVASystem, TiberiumContent.Icon_EVA, "Enable or disable the EVA", SoundDefOf.Mouseover_ButtonToggle);
                row.ToggleableIcon(ref TRUtils.GameSettings().RadiationOverlay, TiberiumContent.Hediff_Radiation, "Toggle the Tiberium Radiation overlay.", SoundDefOf.Mouseover_ButtonToggle);
            }
        }

        [HarmonyPatch(typeof(MainMenuDrawer))]
        [HarmonyPatch(nameof(MainMenuDrawer.DoMainMenuControls))]
        public static class DoMainMenuControlsPatch
        {
            public static float addedHeight = 45f + 7f;
            public static List<ListableOption> OptionList;
            private static MethodInfo ListingOption = SymbolExtensions.GetMethodInfo(() => AdjustList(null));

            static void AdjustList(List<ListableOption> optList)
            {
                var label = "Options".Translate();
                var idx = optList.FirstIndexOf(opt => opt.label == label);
                if (idx > 0 && idx < optList.Count) optList.Insert(idx + 1, new ListableOption("[TR]Dev Tools", delegate ()
                {
                    Find.WindowStack.Add(new Dialog_ToolSelection());
                }, null));
                OptionList = optList;
            }

            static bool Prefix(ref Rect rect, bool anyMapFiles)
            {
                rect = new Rect(rect.x, rect.y, rect.width, rect.height + addedHeight);
                return true;
            }

            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var m_DrawOptionListing = SymbolExtensions.GetMethodInfo(() => OptionListingUtility.DrawOptionListing(Rect.zero, null));

                var instructionsList = instructions.ToList();
                var patched = false;
                for (var i = 0; i < instructionsList.Count; i++)
                {
                    var instruction = instructionsList[i];
                    if (i + 2 < instructionsList.Count)
                    {
                        var checkingIns = instructionsList[i + 2];
                        if (!patched && checkingIns != null && checkingIns.Calls(m_DrawOptionListing))
                        {
                            yield return new CodeInstruction(OpCodes.Ldloc_2);
                            yield return new CodeInstruction(OpCodes.Call, ListingOption);
                            patched = true;
                        }
                    }
                    yield return instruction;
                }
            }
        }

        //Readout
        [HarmonyPatch(typeof(ResourceReadout))]
        [HarmonyPatch(nameof(ResourceReadout.ResourceReadoutOnGUI))]
        public static class ResourceReadoutOnGUIPatch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                MethodInfo helper = AccessTools.Method(typeof(ResourceReadoutOnGUIPatch), nameof(AdjustResourceReadoutDownwards));

                bool patched = false;
                foreach (var code in instructions)
                {
                    yield return code;
                    if (code.opcode == OpCodes.Stloc_0 && !patched)
                    {
                        yield return new CodeInstruction(OpCodes.Ldloc_0); //Rect on stack  
                        yield return new CodeInstruction(OpCodes.Call, helper); //Consumes 1 and returns Rect
                        yield return new CodeInstruction(OpCodes.Stloc_0);
                        patched = true;
                    }
                }

            }

            public static void Postfix(ResourceReadout __instance)
            {
                if (ShouldFix)
                    DrawCredits();
            }

            static Rect AdjustResourceReadoutDownwards(Rect rect)
            {
                if (!ShouldFix) return rect;

                Rect newRect = new Rect(rect);
                newRect.yMin += TotalHeight.Value + 10f;
                return newRect;
            }

            private static bool ShouldFix => GetTiberiumCredits(Find.CurrentMap) > 0;

            static float GetTiberiumCredits(Map map)
            {
                return GetNetwork(map)?.TotalNetworkValue ?? 0;
            }

            private static float? TotalHeight = 120;
            private static float? ResourceReadoutHeight = 60f;

            static Network GetNetwork(Map map)
            {
                return map.Tiberium()?.NetworkInfo[TiberiumDefOf.TiberiumNetwork]?.MainNetworkComponent?.Network;
            }

            static void DrawCredits()
            {
                //
                Rect mainRect = new Rect(5f, 10, 120f, TotalHeight.Value);
                string creditLabel = "TR_Credits".Translate();
                Vector2 creditLabelSize = Text.CalcSize(creditLabel);
                Rect creditLabelRect = new Rect(5, mainRect.y, mainRect.width, creditLabelSize.y + 8);
                Rect readoutRect = new Rect(5, creditLabelRect.yMax, mainRect.width, ResourceReadoutHeight.Value);

                //Main
                //Widgets.DrawWindowBackground(mainRect);
                TRWidgets.DrawColoredBox(mainRect, new Color(1, 1, 1, 0.125f), Color.white, 1);
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(creditLabelRect, creditLabel);
                Text.Anchor = default;
                Widgets.DrawLine(new Vector2(5f, creditLabelRect.yMax), new Vector2(125f, creditLabelRect.yMax), Color.white, 1f);

                ResourceReadoutHeight = TRWidgets.DrawNetworkValueTypeReadout(readoutRect, GameFont.Tiny, -2, GetNetwork(Find.CurrentMap).ContainerSet);

                Text.Font = GameFont.Tiny;
                string totalLabel = "TR_CreditsTotal".Translate(Math.Round(GetTiberiumCredits(Find.CurrentMap)));
                Vector2 totalLabelSize = Text.CalcSize(totalLabel);
                Rect totalLabelRect = new Rect(10f, readoutRect.yMax, mainRect.width, totalLabelSize.y);
                Widgets.Label(totalLabelRect, totalLabel);
                Text.Font = default;

                TotalHeight = totalLabelRect.yMax - mainRect.y;
                //
            }
        }
    }
}
