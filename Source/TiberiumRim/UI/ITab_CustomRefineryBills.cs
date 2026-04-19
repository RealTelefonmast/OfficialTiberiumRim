using System.Collections.Generic;
using System.Linq;
using RimWorld;
using TiberiumRim;
using TR.Networks.TiberiumNetwork;
using UnityEngine;
using Verse;

namespace TR;

public class ITab_CustomRefineryBills : ITab
{
    public static readonly float MarketPriceFactor = 2.4f;
    public static readonly float WorkAmountFactor = 48;
    private static readonly Vector2 WinSize = new(800, 500);
    private static readonly float resourceSize = 26;

    private static float maxLabelWidth;

    //Scrollers
    private Vector2 billCreationResourceScroller;
    private Vector2 billReadourScroller;

    public ITab_CustomRefineryBills()
    {
        size = WinSize;
        labelKey = "TR_TibResourceRefiner";
    }

    public static IEnumerable<ThingDef> Metals => DefDatabase<ThingDef>.AllDefs
        .Where(t => t.mineable && t.building.mineableThing != null && t.building.mineableThing.IsMetal)
        .Select(t => t.building.mineableThing);

    public CompTNS_Crafter CrafterComp => SelThing.TryGetComp<CompTNS_Crafter>();
    public TiberiumBillStack BillStack => CrafterComp.BillStack;

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
        TiberiumBillUtility.Clipboard = null;
    }

    public override void FillTab()
    {
        Text.Font = GameFont.Small;
        var mainRect = new Rect(0, 24, WinSize.x, WinSize.y - 24).ContractedBy(10);
        var leftPart = mainRect.LeftPart(0.6f);
        var rightPart = mainRect.RightPart(0.4f);
        var pasteButton = new Rect(rightPart.x, rightPart.y - 22, 22, 22);

        //Left Part
        BillCreation(leftPart.ContractedBy(5));
        //Right Part
        DrawBillInfo(rightPart.ContractedBy(5));
        //Paste Option
        if (TiberiumBillUtility.Clipboard != null)
        {
            if (Widgets.ButtonImage(pasteButton, TiberiumContent.Paste)) BillStack.PasteFromClipBoard();
        }
        else
        {
            GUI.color = Color.gray;
            Widgets.DrawTextureFitted(pasteButton, TiberiumContent.Paste, 1);
            GUI.color = Color.white;
        }
    }

    private void DrawBillInfo(Rect rect)
    {
        Widgets.DrawMenuSection(rect);
        GUI.BeginGroup(rect);

        var outRect = new Rect(0, 0, rect.width, rect.height);
        var viewRect = new Rect(0, 0, rect.width, CrafterComp.billStack.Bills.Sum(a => a.DrawHeight));
        Widgets.BeginScrollView(outRect, ref billReadourScroller, viewRect, false);
        float curY = 0;
        for (var index = 0; index < CrafterComp.billStack.Count; index++)
        {
            var bill = CrafterComp.billStack[index];
            bill.DrawBill(new Rect(0, curY, rect.width, bill.DrawHeight), index);
            curY += bill.DrawHeight;
        }

        Widgets.EndScrollView();
        GUI.EndGroup();
    }

    private void BillCreation(Rect rect)
    {
        var topPart = rect.TopPart(0.65f);
        var bottomPart = rect.BottomPart(0.35f);

        //TOP PART
        topPart = topPart.ContractedBy(5f);
        GUI.BeginGroup(topPart);

        var label1 = "Desired Resources";
        var label2 = "Credit Cost (" + "Market Value".Colorize(Color.yellow) +
                     " * Credit Factor".Colorize((string)TRMats.Orange) + ")";
        var label1H = Text.CalcHeight(label1, rect.width);
        var resourceWidth = resourceSize + maxLabelWidth + 60;
        var label1Rect = new Rect(0, 0, rect.width, label1H);
        var label2Rect = new Rect(resourceWidth + 5, 0, rect.width - (resourceWidth + 5), label1H);
        Widgets.Label(label1Rect, label1);
        Widgets.Label(label2Rect, label2);
        //Wanted Resources
        var resourceRect = new Rect(0, label1H + 5, rect.width, topPart.height - label1H);
        var scrollRect = new Rect(0, label1H + 5, rect.width, BillStack.MetalAmount.Count * (resourceSize + 4));

        Widgets.BeginScrollView(resourceRect, ref billCreationResourceScroller, scrollRect, false);
        var curY = label1H + 5;
        for (var i = 0; i < Metals.Count(); i++)
        {
            ResourceRow(new Rect(0, curY, rect.width, resourceSize), Metals.ElementAt(i), i);
            curY += resourceSize + 4;
        }

        Widgets.EndScrollView();
        GUI.EndGroup();

        //BOTTOM PART
        BillCreationInfo(bottomPart);
    }

    private void BillCreationInfo(Rect rect)
    {
        Widgets.DrawMenuSection(rect);
        rect = rect.ContractedBy(5f);
        GUI.BeginGroup(rect);
        var nameLabel = "Bill Name: ";
        var workLabel = "Work To Do: " + BillStack.TotalWorkAmount;
        var tiberiumCostLabel = "Cost: " + BillStack.TotalCost;
        var nameLabelSize = Text.CalcSize(nameLabel);
        var workLabelSize = Text.CalcSize(workLabel);
        var tiberiumCostLabelSize = Text.CalcSize(tiberiumCostLabel);
        var nameLabelRect = new Rect(0, 0, nameLabelSize.x, nameLabelSize.y);
        var nameFieldRect = new Rect(nameLabelRect.xMax, 0, rect.width / 2 - nameLabelRect.width,
            nameLabelRect.height);

        var workLabelRect = new Rect(0, nameLabelRect.yMax + 5, workLabelSize.x, workLabelSize.y);
        var tiberiumCostLabelRect = new Rect(0, workLabelRect.yMax, tiberiumCostLabelSize.x, tiberiumCostLabelSize.y);
        var addButtonRect = new Rect(rect.width - 80, rect.height - 30, 80, 30);

        Widgets.Label(nameLabelRect, nameLabel);
        BillStack.billName = Widgets.TextField(nameFieldRect, BillStack.billName);

        Widgets.Label(workLabelRect, workLabel);
        Widgets.Label(tiberiumCostLabelRect, tiberiumCostLabel);

        if (Widgets.ButtonText(addButtonRect, "Add Bill")) BillStack.CreateNewBill();

        GUI.EndGroup();
    }

    private void ResourceRow(Rect rect, ThingDef resource, int index)
    {
        var iconRect = new Rect(rect.xMin, rect.y, resourceSize, resourceSize);
        var labelSize = Text.CalcSize(resource.LabelCap);
        if (labelSize.x > maxLabelWidth) maxLabelWidth = labelSize.x;

        var labelRect = new Rect(iconRect.xMax, rect.y, labelSize.x, resourceSize);
        var fieldRect = new Rect(iconRect.xMax + maxLabelWidth + 5, rect.y, 60, resourceSize);

        Widgets.ThingIcon(iconRect, resource);
        Text.Anchor = TextAnchor.MiddleLeft;
        Widgets.Label(labelRect, resource.LabelCap);
        Text.Anchor = default;

        var temp = BillStack.MetalAmount[resource];
        Widgets.TextFieldNumeric(fieldRect, ref temp, ref BillStack.textBuffers[index], 0,
            resource.stackLimit *
            2); //(int)Widgets.HorizontalSlider(sliderRect, MetalAmount[resource], 0, 100, false, default, default, default, 1);
        BillStack.MetalAmount[resource] = temp;

        CostLabel(new Vector2(fieldRect.xMax + 5, fieldRect.y), resource);

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

    // × 2400 (BaseMarketValue * Multiplier)
    private void CostLabel(Vector2 pos, ThingDef resource)
    {
        var totalCost = "× " + BillStack.MetalAmount[resource] * resource.BaseMarketValue * MarketPriceFactor;
        var marketValue = " (" + (resource.BaseMarketValue + " ").Colorize(Color.yellow);
        var multiplier = ("* " + MarketPriceFactor).Colorize((string)TRMats.Orange) + ")";
        var label1Size = Text.CalcSize(totalCost);
        var label2Size = Text.CalcSize(marketValue);
        var label3Size = Text.CalcSize(multiplier);
        var totalCostRect = new Rect(pos.x, pos.y, label1Size.x, label1Size.y);
        var baseMarketValueRect = new Rect(totalCostRect.xMax, pos.y, label2Size.x, label2Size.y);
        var multiplierRect = new Rect(baseMarketValueRect.xMax, pos.y, label3Size.x, label3Size.y);

        Widgets.Label(totalCostRect, totalCost);
        Widgets.Label(baseMarketValueRect, marketValue);
        Widgets.Label(multiplierRect, multiplier);
        TooltipHandler.TipRegion(totalCostRect, "");
        TooltipHandler.TipRegion(baseMarketValueRect, resource.LabelCap + "'s market value.");
        TooltipHandler.TipRegion(multiplierRect, "Tiberium Cost Factor");
    }
}