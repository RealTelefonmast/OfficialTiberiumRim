using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using HarmonyLib;
using RimWorld;
using TeleCore.Logging;
using TeleCore.Mod;
using TeleCore.Network.Bills;
using TeleCore.Rendering.Tools;
using TeleCore.Rendering.UI.SpecialSubMenu;
using TeleCore.RWExtended.UI.Commands;
using TeleCore.Static;
using TeleCore.Utils;
using UnityEngine;
using Verse;
//TODO: Resolve TiberiumRim namespace references after consolidation
//using TiberiumRim;

namespace TeleCore.Absorbed.RWLib.Patches;

public class ListableOption_Tele : ListableOption
{
    public ListableOption_Tele(string label, Action action, string uiHighlightTag = null) : base(label, action,
        uiHighlightTag)
    {
    }

    public override float DrawOption(Vector2 pos, float width)
    {
        var b = Text.CalcHeight(label, width);
        var num = Mathf.Max(minHeight, b);
        var rect = new Rect(pos.x, pos.y, width, num);

        var atlas = TeleContent.ButtonBGAtlas;
        if (Mouse.IsOver(rect))
        {
            atlas = TeleContent.ButtonBGAtlasMouseover;
            if (Input.GetMouseButton(0)) atlas = TeleContent.ButtonBGAtlasClick;
        }

        Widgets.DrawAtlas(rect, atlas);

        Text.Anchor = TextAnchor.MiddleCenter;
        Widgets.Label(rect, label);
        Text.Anchor = default;


        if (Widgets.ButtonInvisible(rect))
            action();

        if (uiHighlightTag != null)
            UIHighlighter.HighlightOpportunity(rect, uiHighlightTag);
        return num;
    }
}

internal static class UIPatches
{
    [HarmonyPatch(typeof(DesignationCategoryDef))]
    [HarmonyPatch(nameof(DesignationCategoryDef.Visible), MethodType.Getter)]
    internal static class DesignationCategoryDefVisiblePatch
    {
        public static bool Prefix(DesignationCategoryDef __instance, ref bool __result)
        {
            if (__instance is SubMenuDesignationCategoryDef def)
                if (def.isDebug && !DebugSettings.godMode)
                {
                    __result = false;
                    return false;
                }

            return true;
        }
    }

    [HarmonyPatch(typeof(MainTabWindow_Architect))]
    [HarmonyPatch(nameof(MainTabWindow_Architect.ClickedCategory))]
    internal static class ClickedCategoryPatch
    {
        private static void Postfix(ArchitectCategoryTab Pan, MainTabWindow_Architect __instance)
        {
            var subMenuDes = Pan.def.AllResolvedDesignators.Find(d => d is Designator_SubBuildMenu);
            if (subMenuDes is Designator_SubBuildMenu subMenu)
            {
                //var opening = __instance.selectedDesPanel != Pan;
                subMenu.Toggle_Menu();
            }
        }
    }

    [HarmonyPatch(typeof(MainMenuDrawer))]
    [HarmonyPatch(nameof(MainMenuDrawer.DoMainMenuControls))]
    internal static class DoMainMenuControlsPatch
    {
        private static readonly float addedHeight = 45f + 7f;
        private static List<ListableOption> OptionList;
        private static readonly MethodInfo ListingOption = SymbolExtensions.GetMethodInfo(() => AdjustList(null));

        private static void AdjustList(List<ListableOption> optList)
        {
            if (!TeleCoreMod.Settings.showToolsInMainMenu) return;

            try
            {
                var label = "Options".Translate();
                var idx = optList.FirstIndexOf(opt => opt.label == label);
                if (idx > 0 && idx < optList.Count)
                    optList.Insert(idx + 1,
                        new ListableOption_Tele(RWStringCache.TeleTools,
                            delegate { Find.WindowStack.Add(new Dialog_ToolSelection()); }));
                OptionList = optList;
            }
            catch (Exception ex)
            {
                TLog.Message($"{ex}");
            }
        }

        private static bool Prefix(ref Rect rect, bool anyMapFiles)
        {
            rect = new Rect(rect.x, rect.y, rect.width, rect.height + addedHeight);
            return true;
        }

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var m_DrawOptionListing =
                SymbolExtensions.GetMethodInfo(() => OptionListingUtility.DrawOptionListing(Rect.zero, null));

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

    //TODO: Move to FlowCore
    //Dialogs
    [HarmonyPatch(typeof(Dialog_BillConfig))]
    [HarmonyPatch(nameof(Dialog_BillConfig.DoWindowContents))]
    internal static class Dialog_BillConfigDoWindowContentsPatch
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            //MethodInfo methodFinder = AccessTools.Method(typeof(StringBuilder), nameof(StringBuilder.AppendLine));
            var methodFinder_ToString = AccessTools.Method(typeof(object), nameof(ToString));
            var helper = AccessTools.Method(typeof(Dialog_BillConfigDoWindowContentsPatch), nameof(WriteNetworkCost));

            CodeInstruction lastInstruction = null;

            var finalPatched = false;
            foreach (var code in instructions)
            {
                if (code is { operand: not null })
                {
                    //Finds StringBuilder.ToString
                    var lastOperand = (lastInstruction?.operand as LocalBuilder)?.LocalType == typeof(StringBuilder);
                    var codeOperand = code.operand.Equals(methodFinder_ToString);
                    if (codeOperand && lastOperand)
                        if (!finalPatched)
                        {
                            //Current Stack: StringBuilder local field
                            //Loads Instance Local Field Onto Stack
                            yield return new CodeInstruction(OpCodes.Ldarg_0);

                            //Calls WriteNetworkCost(stringbuilder, instance)
                            yield return new CodeInstruction(OpCodes.Call, helper);

                            //Re-return stringbuilder onto stack
                            yield return lastInstruction.Clone();
                            finalPatched = true;
                        }
                }

                lastInstruction = code;
                yield return code;
            }
        }

        private static void WriteNetworkCost(StringBuilder stringBuilder, Dialog_BillConfig instance)
        {
            if (instance.bill is Bill_Production_Network tBill && tBill.def.networkCost.Valid)
            {
                stringBuilder.AppendLine("Network Cost:");
                foreach (var cost in tBill.def.networkCost.Cost.SpecificCosts)
                    stringBuilder.AppendLine(
                        $" - {cost.valueDef.LabelCap.Colorize(cost.valueDef.valueColor)}: {cost.value}");

                stringBuilder.AppendLine($"BaseShouldBeDone: {tBill.BaseShouldDo}");
                stringBuilder.AppendLine($"ShouldBeDone: {tBill.ShouldDoNow()}");
                stringBuilder.AppendLine($"CompTNW: {tBill.CompTNW is { IsPowered: true }}");
                stringBuilder.AppendLine($"def.CanPay: {tBill.def.networkCost.CanPayWith(tBill.CompTNW)}");
            }
        }
    }

    [HarmonyPatch(typeof(global::RimWorld.PlaySettings))]
    [HarmonyPatch(nameof(global::RimWorld.PlaySettings.DoPlaySettingsGlobalControls))]
    public static class PlaySettingsPatch
    {
        public static void Postfix(WidgetRow row, bool worldView)
        {
            foreach (var setting in StaticData.PlaySettings)
            {
                if (setting.Visible)
                    if ((worldView && setting.ShowOnWorldView) || (!worldView && setting.ShowOnMapView))
                        if (row.ButtonIcon(setting.ActiveIcon, tooltip: setting.Description))
                            setting.Toggle();
            }
        }
    }

    [HarmonyPatch(typeof(GizmoGridDrawer))]
    [HarmonyPatch(nameof(GizmoGridDrawer.DrawGizmoGrid))]
    public static class GizmoGridDrawer_DrawGizmoGrid_Patch
    {
        public static bool Prefix(IEnumerable<Gizmo> gizmos, ref float startX, ref Gizmo mouseoverGizmo,
            Func<Gizmo, bool> customActivatorFunc = null, Func<Gizmo, bool> highlightFunc = null,
            Func<Gizmo, bool> lowlightFunc = null)
        {
            var array = gizmos as Gizmo[] ?? gizmos.ToArray();
            if (array.FirstOrFallback(g => g is Gizmo_NetworkOverview) is Gizmo_NetworkOverview network)
            {
                startX = network.GetWidthSpecial() + startX;
            }
            return true;
        }
    }


    [HarmonyPatch(typeof(DesignatorManager))]
    [HarmonyPatch("Deselect")]
    public static class DeselectPatch
    {
        public static bool Prefix(DesignatorManager __instance)
        {
            if (__instance.SelectedDesignator is Designator_Extended { MustStaySelected: true })
                return false;
            return true;
        }
    }
}