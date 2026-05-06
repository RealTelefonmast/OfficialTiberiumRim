using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using TeleCore.Defs;
using TeleCore.Types;
using TeleCore.Types.Exposables;
using TeleCore.Types.Structs;
using TeleCore.Types.Utils;
using UnityEngine;
using Verse;

namespace TeleCore.UI;

public class ITab_CustomNetworkBills : ITab
{
    private static readonly Vector2 WinSize = new(800, 500);
    private static readonly float resourceSize = 26;

    private static float maxLabelWidth;

    //Scrollers
    private Vector2 billCreationResourceScroller;
    private Vector2 billReadourScroller;
    private DefValueStack<NetworkValueDef, float> cachedCustomCost;

    //Cachery
    private bool inputDirty;

    private Vector2 presetScrollVec = Vector2.zero;

    public ITab_CustomNetworkBills()
    {
        size = WinSize;
        labelKey = Translations.NetworkStrings.NetworkBillsITab;
    }

    public Comp_NetworkBillsCrafter CrafterComp => SelThing.TryGetComp<Comp_NetworkBillsCrafter>();
    public NetworkBillStack BillStack => CrafterComp.BillStack;

    public List<CustomRecipeRatioDef> Ratios => CrafterComp.Props.UsedRatioDefs;
    public List<CustomRecipePresetDef> Presets => CrafterComp.Props.UsedPresetDefs;

    public CustomNetworkBill ClipBoard =>
        ClipBoardUtility.TryGetClipBoard<CustomNetworkBill>(RWStringCache.NetworkBillClipBoard);

    private CustomBillTab SelTab { get; set; }

    public override void OnOpen()
    {
        base.OnOpen();
    }

    public override void TabUpdate()
    {
        base.TabUpdate();
    }

    public override void CloseTab()
    {
        base.CloseTab();
    }

    public override void Notify_ClearingAllMapsMemory()
    {
        base.Notify_ClearingAllMapsMemory();
        ClipBoardUtility.TrySetClipBoard<CustomNetworkBill>(RWStringCache.NetworkBillClipBoard, null);
    }

    //Drawing
    public override void FillTab()
    {
        Text.Font = GameFont.Small;
        var mainRect = new Rect(0, 24, WinSize.x, WinSize.y - 24).ContractedBy(10);
        var leftPart = mainRect.LeftPart(0.6f);
        var rightPart = mainRect.RightPart(0.4f);

        var leftArea = leftPart.ContractedBy(5);
        var tabRect = new Rect(leftArea.x, leftArea.y, leftArea.width, 32);
        var contentRect = new Rect(leftArea.x, leftArea.y, leftArea.width, leftArea.height);

        var pasteButton = new Rect(rightPart.x, rightPart.y - 22, 22, 22);

        //Left Part
        //Draw Tabs
        var tabs = new List<TabRecord>();
        tabs.Add(new TabRecord("TR_CustomBillPresetBills".Translate(), delegate { SelTab = CustomBillTab.PresetBills; },
            SelTab == CustomBillTab.PresetBills));
        tabs.Add(new TabRecord("TR_CustomBillCustom".Translate(), delegate { SelTab = CustomBillTab.CustomBills; },
            SelTab == CustomBillTab.CustomBills));
        //Widgets.DrawLine(new Vector2(leftArea.x, leftArea.y), new Vector2(leftArea.xMax, leftArea.y), TRColor.White025, 1);

        TabDrawer.DrawTabs(tabRect, tabs);
        switch (SelTab)
        {
            case CustomBillTab.PresetBills:
                BillSelection(contentRect);
                break;
            case CustomBillTab.CustomBills:
                BillCreation(contentRect);
                break;
        }

        //Right Part
        DrawBillReadout(rightPart.ContractedBy(5));

        //Paste Option
        var clipBoardValue = ClipBoard;
        if (clipBoardValue != null)
        {
            if (Widgets.ButtonImage(pasteButton, TeleContent.Paste)) BillStack.PasteFromClipBoard(clipBoardValue);
        }
        else
        {
            GUI.color = Color.gray;
            Widgets.DrawTextureFitted(pasteButton, TeleContent.Paste, 1);
            GUI.color = Color.white;
        }

        //Draw Details
        var detailRect = new Rect(TabRect.xMax, TabRect.y, 200, WinSize.y);
        BillStack.TryDrawBillDetails(detailRect);
    }

    private void DrawBillReadout(Rect inRect)
    {
        var readoutRect = inRect;

        Widgets.DrawMenuSection(inRect);
        var viewRect = new Rect(inRect.x, inRect.y, readoutRect.width,
            CrafterComp.billStack.Bills.Sum(a => a.DrawHeight));
        Widgets.BeginScrollView(readoutRect, ref billReadourScroller, viewRect, false);
        {
            var curY = inRect.y;
            for (var index = 0; index < CrafterComp.billStack.Count; index++)
            {
                var bill = CrafterComp.billStack[index];
                var billRect = new Rect(inRect.x, curY, readoutRect.width, bill.DrawHeight);
                bill.DrawBill(billRect, index);
                if (bill == BillStack.CurrentBill) TWidgets.DrawBox(billRect, TColor.White05, 2);

                curY += bill.DrawHeight;
            }
        }
        Widgets.EndScrollView();
    }

    //Preset Tab
    private void BillSelection(Rect rect)
    {
        Widgets.DrawBoxSolid(rect, TColor.LightBlack);
        TWidgets.DrawListedPart(rect, ref presetScrollVec, Presets, DrawPresetOption, GetListingHeight);
    }

    private UIPartSizes GetListingHeight(CustomRecipePresetDef presetDef)
    {
        var uiSizes = new UIPartSizes(3);
        var labelSize = Text.CalcSize(presetDef.LabelCap);
        var costLabelSize = Text.CalcSize(presetDef.CostLabel);
        uiSizes.Register(0, labelSize.y); //LabelSize

        float resultListHeight = (24 + 5) * presetDef.Results.Count;
        var labelHeight = labelSize.y * 2;
        uiSizes.Register(1, labelHeight > resultListHeight ? labelHeight : resultListHeight); //Content Size
        //uiSizes.Register(2, (5 * 2) + 30); //Padding
        uiSizes.Register(2, costLabelSize.y); // CostLabel
        return uiSizes;
    }

    private void DrawPresetOption(Rect rect, UIPartSizes sizes, CustomRecipePresetDef presetDef)
    {
        var drawRect = rect.ContractedBy(5);
        var contentRect = new Rect(rect.x, rect.y + sizes[0], rect.width, sizes[1]).ContractedBy(5);
        var leftRect = contentRect.LeftHalf();

        Widgets.Label(drawRect, presetDef.LabelCap);

        //Draw Result
        Widgets.BeginGroup(leftRect);
        //List
        float curY = 0;
        foreach (var result in presetDef.Results)
        {
            var row = new WidgetRow(0, curY, UIDirection.RightThenDown);
            row.Icon(result.ThingDef.uiIcon, result.ThingDef.description);
            row.Label($"×{result.Count}");
            curY += 24 + 5;
        }

        Widgets.EndGroup();

        //Draw Cost
        var costLabelRect = new Rect(drawRect.x, contentRect.yMax + 5, contentRect.width, sizes[2]);
        Widgets.Label(costLabelRect, $"Cost: {presetDef.CostLabel}");

        TWidgets.DrawBoxHighlightIfMouseOver(rect);
        if (Widgets.ButtonInvisible(rect)) BillStack.CreateBillFromDef(presetDef);
    }

    //Custom Tab
    private void BillCreation(Rect rect)
    {
        var topPart = rect.TopPart(0.65f);
        var bottomPart = rect.BottomPart(0.35f);

        //TOP PART
        topPart = topPart.ContractedBy(5f);
        Widgets.BeginGroup(topPart);

        var label1 = "Desired Resource";
        var label2 = "Elemental Ratio";
        var label1H = Text.CalcHeight(label1, rect.width);
        var resourceWidth = resourceSize + maxLabelWidth + 60;
        var label1Rect = new Rect(0, 0, rect.width, label1H);
        var label2Rect = new Rect(resourceWidth + 5, 0, rect.width - (resourceWidth + 5), label1H);
        Widgets.Label(label1Rect, label1);
        Widgets.Label(label2Rect, label2);
        //Wanted Resources
        var resourceRect = new Rect(0, label1H + 5, rect.width, topPart.height - label1H);
        var scrollRect = new Rect(0, label1H + 5, rect.width,
            BillStack.RequestedAmount.Count * (resourceSize + 4));

        Widgets.BeginScrollView(resourceRect, ref billCreationResourceScroller, scrollRect, false);
        var curY = label1H + 5;
        for (var i = 0; i < Ratios.Count; i++)
        {
            var recipe = Ratios[i];
            if (recipe.hidden) continue;
            ResourceRow(new Rect(0, curY, rect.width, resourceSize), recipe, i);
            curY += resourceSize + 4;
        }

        Widgets.EndScrollView();
        Widgets.EndGroup();

        //BOTTOM PART
        BillCreationInfo(bottomPart);
    }


    //Custom Bill Preview
    private void BillCreationInfo(Rect rect)
    {
        Widgets.DrawMenuSection(rect);
        rect = rect.ContractedBy(5f);
        Widgets.BeginGroup(rect);
        string nameLabel = "TR_BillName".Translate();
        var workLabel = "Work To Do: " + BillStack.TotalWorkAmount;
        var tiberiumCostLabel = $"Cost: {NetworkBillUtility.CostLabel(TryGetCachedCost())}";
        var nameLabelSize = Text.CalcSize(nameLabel);
        var workLabelSize = Text.CalcSize(workLabel);
        var tiberiumCostLabelSize = Text.CalcSize(tiberiumCostLabel);
        var nameLabelRect = new Rect(0, 0, nameLabelSize.x, nameLabelSize.y);
        var nameFieldRect = new Rect(nameLabelRect.xMax, 0, rect.width / 2 - nameLabelRect.width,
            nameLabelRect.height);

        var workLabelRect = new Rect(0, nameLabelRect.yMax + 5, workLabelSize.x, workLabelSize.y);
        var tiberiumCostLabelRect =
            new Rect(0, workLabelRect.yMax, tiberiumCostLabelSize.x, tiberiumCostLabelSize.y);
        var addButtonRect = new Rect(rect.width - 80, rect.height - 30, 80, 30);

        Widgets.Label(nameLabelRect, nameLabel);
        BillStack.billName = Widgets.TextField(nameFieldRect, BillStack.billName);

        Widgets.Label(workLabelRect, workLabel);
        Widgets.Label(tiberiumCostLabelRect, tiberiumCostLabel);

        if (Widgets.ButtonText(addButtonRect, "TR_AddBill".Translate())) BillStack.TryCreateNewBill();

        Widgets.EndGroup();
    }

    private void ResourceRow(Rect rect, CustomRecipeRatioDef recipeRatio, int index)
    {
        var resource = recipeRatio.result;
        var iconRect = new Rect(rect.xMin, rect.y, resourceSize, resourceSize);
        var labelSize = Text.CalcSize(resource.LabelCap);
        if (labelSize.x > maxLabelWidth) maxLabelWidth = labelSize.x;

        var labelRect = new Rect(iconRect.xMax, rect.y, labelSize.x, resourceSize);
        var fieldRect = new Rect(iconRect.xMax + maxLabelWidth + 5, rect.y, 60, resourceSize);

        Widgets.ThingIcon(iconRect, resource);
        Text.Anchor = TextAnchor.MiddleLeft;
        Widgets.Label(labelRect, resource.LabelCap);
        Text.Anchor = default;

        int temp, compare = temp = BillStack.RequestedAmount[recipeRatio];
        Widgets.TextFieldNumeric(fieldRect, ref temp, ref BillStack.textBuffers[index], 0,
            resource.stackLimit *
            2); //(int)Widgets.HorizontalSlider(sliderRect, MetalAmount[resource], 0, 100, false, default, default, default, 1);
        BillStack.RequestedAmount[recipeRatio] = temp;
        if (compare != temp) inputDirty = true;

        CostLabel(new Vector2(fieldRect.xMax + 5, fieldRect.y), recipeRatio);

        //Rect buttonAdd = new Rect(fieldRect.xMax, rect.y, 30, resourceSize/2);
        //Rect buttonRemove = new Rect(fieldRect.xMax, rect.y+15, 30, resourceSize/2);
        /*
        if (Widgets.ButtonText(buttonAdd, "▲"))
        {
            MetalAmount[resource] = Mathf.Clamp(MetalAmount[resource] + 10, 0, resource.stackLimit * 2);
            textBuffers[index] = MetalAmount[resource].ToString();
        }
        if (Widgets.ButtonText(buttonRemove, "▼"))
        {
            MetalAmount[resource] = Mathf.Clamp(MetalAmount[resource] - 10, 0, resource.stackLimit * 2);
            textBuffers[index] = MetalAmount[resource].ToString();
        }
        */
    }

    public DefValueStack<NetworkValueDef, float> TryGetCachedCost()
    {
        if (!inputDirty && cachedCustomCost != null) return cachedCustomCost;
        inputDirty = false;
        BillStack.TotalCost = cachedCustomCost = NetworkBillUtility.ConstructCustomCostStack(BillStack.RequestedAmount);
        BillStack.ByProducts = NetworkBillUtility.ConstructCustomCostStack(BillStack.RequestedAmount, true);

        return cachedCustomCost;
    }

    // public DefFloat<NetworkValueDef>[] TryGetCachedCost()
    // {
    //     if (!inputDirty && cachedCustomCost != null) return cachedCustomCost;
    //     inputDirty = false;
    //     BillStack.TotalCost = cachedCustomCost = NetworkBillUtility.ConstructCustomCostStack(BillStack.RequestedAmount);
    //
    //     return cachedCustomCost;
    // }

    //
    private static void CostLabel(Vector2 pos, CustomRecipeRatioDef recipeRatio)
    {
        var sb = new StringBuilder();
        sb.Append(" x (");
        for (var i = 0; i < recipeRatio.inputRatio.Count; i++)
        {
            var input = recipeRatio.inputRatio[i];
            sb.Append($"{input.Value}{input.Def.labelShort.Colorize(input.Def.valueColor)}");
            if (i + 1 < recipeRatio.inputRatio.Count)
                sb.Append(" ");
        }

        sb.Append(")");
        var atomicTotal = sb.ToString();
        var label0Size = Text.CalcSize(atomicTotal);
        var atomicTotalRect = new Rect(pos.x, pos.y, label0Size.x, label0Size.y);
        Widgets.Label(atomicTotalRect, atomicTotal);
    }

    private enum CustomBillTab
    {
        PresetBills,
        CustomBills
    }
}