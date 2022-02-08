using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class AtomicRecipeDef : Def
    {
        public List<DefFloat<NetworkValueDef>> inputRatio;
        public ThingDef result;
    }

    public class ITab_CustomRefineryBills : ITab
    {
        public static readonly float MarketPriceFactor = 2.4f;
        public static readonly float WorkAmountFactor = 10;
        private static readonly Vector2 WinSize = new Vector2(800, 500);
        private static readonly float resourceSize = 26;

        private static float maxLabelWidth = 0;

        //public static IEnumerable<ThingDef> Metals => DefDatabase<ThingDef>.AllDefs.Where(t => t.mineable && t.building.mineableThing != null && t.building.mineableThing.IsMetal).Select(t => t.building.mineableThing);

        public static IEnumerable<AtomicRecipeDef> Recipes => DefDatabase<AtomicRecipeDef>.AllDefs;

        //Scrollers
        private Vector2 billCreationResourceScroller = new Vector2();
        private Vector2 billReadourScroller = new Vector2();

        public Comp_NetworkStructureCrafter CrafterComp => SelThing.TryGetComp<Comp_NetworkStructureCrafter>();
        public NetworkBillStack BillStack => CrafterComp.BillStack;

        public ITab_CustomRefineryBills()
        {
            this.size = WinSize;
            this.labelKey = "TR_TibResourceRefiner";
        }

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
            ClipBoardUtility.Clipboard = null;
        }

        public override void FillTab()
        {
            Text.Font = GameFont.Small;
            Rect mainRect = new Rect(0, 24, WinSize.x, WinSize.y - 24).ContractedBy(10);
            Rect leftPart = mainRect.LeftPart(0.6f);
            Rect rightPart = mainRect.RightPart(0.4f);
            Rect pasteButton = new Rect(rightPart.x, rightPart.y - 22, 22, 22);

            //Left Part
            BillCreation(leftPart.ContractedBy(5));
            //Right Part
            DrawBillInfo(rightPart.ContractedBy(5));
            //Paste Option
            if (ClipBoardUtility.Clipboard != null)
            {
                if (Widgets.ButtonImage(pasteButton, TiberiumContent.Paste))
                {
                    BillStack.PasteFromClipBoard();
                }
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
            
            Rect outRect = new Rect(0,0, rect.width, rect.height);
            Rect viewRect = new Rect(0,0, rect.width, CrafterComp.billStack.Bills.Sum(a => a.DrawHeight));
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
            Rect topPart = rect.TopPart(0.65f);
            Rect bottomPart = rect.BottomPart(0.35f);

            //TOP PART
            topPart = topPart.ContractedBy(5f);
            GUI.BeginGroup(topPart);

            string label1 = "Desired Resource";
            string label2 = $"Elemental Ratio";
            float label1H = Text.CalcHeight(label1, rect.width);
            float resourceWidth = resourceSize + maxLabelWidth + 60;
            Rect label1Rect = new Rect(0, 0, rect.width, label1H);
            Rect label2Rect = new Rect(resourceWidth + 5, 0, rect.width - (resourceWidth + 5), label1H);
            Widgets.Label(label1Rect, label1);
            Widgets.Label(label2Rect, label2);
            //Wanted Resources
            Rect resourceRect = new Rect(0, label1H + 5, rect.width, topPart.height - label1H);
            Rect scrollRect = new Rect(0, label1H + 5, rect.width, BillStack.RequestedAmount.Count * (resourceSize + 4));

            Widgets.BeginScrollView(resourceRect, ref billCreationResourceScroller, scrollRect, false);
            float curY = label1H + 5;
            for (int i = 0; i < Recipes.Count(); i++)
            {
                ResourceRow(new Rect(0, curY, rect.width, resourceSize), Recipes.ElementAt(i), i);
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
            string nameLabel = "Bill Name: ";
            string workLabel = "Work To Do: " + BillStack.TotalWorkAmount;
            string tiberiumCostLabel = "Cost: " + BillStack.TotalCost;
            Vector2 nameLabelSize = Text.CalcSize(nameLabel);
            Vector2 workLabelSize = Text.CalcSize(workLabel);
            Vector2 tiberiumCostLabelSize = Text.CalcSize(tiberiumCostLabel);
            Rect nameLabelRect = new Rect(0, 0, nameLabelSize.x, nameLabelSize.y);
            Rect nameFieldRect = new Rect(nameLabelRect.xMax, 0, (rect.width / 2) - nameLabelRect.width,
                nameLabelRect.height);

            Rect workLabelRect = new Rect(0, nameLabelRect.yMax + 5, workLabelSize.x, workLabelSize.y);
            Rect tiberiumCostLabelRect = new Rect(0, workLabelRect.yMax, tiberiumCostLabelSize.x, tiberiumCostLabelSize.y);
            Rect addButtonRect = new Rect(rect.width - 80, rect.height - 30, 80, 30);

            Widgets.Label(nameLabelRect, nameLabel);
            BillStack.billName = Widgets.TextField(nameFieldRect, BillStack.billName);

            Widgets.Label(workLabelRect, workLabel);
            Widgets.Label(tiberiumCostLabelRect, tiberiumCostLabel);

            if (Widgets.ButtonText(addButtonRect, "Add Bill"))
            {
                BillStack.CreateNewBill();
            }

            GUI.EndGroup();
        }

        private void ResourceRow(Rect rect, AtomicRecipeDef recipe, int index)
        {
            var resource = recipe.result;
            Rect iconRect = new Rect(rect.xMin, rect.y, resourceSize, resourceSize);
            Vector2 labelSize = Text.CalcSize(resource.LabelCap);
            if (labelSize.x > maxLabelWidth) maxLabelWidth = labelSize.x;

            Rect labelRect = new Rect(iconRect.xMax, rect.y, labelSize.x, resourceSize);
            Rect fieldRect = new Rect(iconRect.xMax + maxLabelWidth + 5, rect.y, 60, resourceSize);

            Widgets.ThingIcon(iconRect, resource);
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(labelRect, resource.LabelCap);
            Text.Anchor = default;

            var temp = BillStack.RequestedAmount[recipe];
            Widgets.TextFieldNumeric<int>(fieldRect, ref temp, ref BillStack.textBuffers[index], 0, resource.stackLimit * 2); //(int)Widgets.HorizontalSlider(sliderRect, MetalAmount[resource], 0, 100, false, default, default, default, 1);
            BillStack.RequestedAmount[recipe] = temp;

            CostLabel(new Vector2(fieldRect.xMax + 5, fieldRect.y), recipe);

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
        private void CostLabel(Vector2 pos, AtomicRecipeDef recipe)
        {
            StringBuilder sb = new StringBuilder();
            var amount = BillStack.RequestedAmount[recipe];
            sb.Append("(");
            foreach (var input in recipe.inputRatio)
            {
                sb.Append($"{amount * input.value}{input.def.labelShort.Colorize(input.def.valueColor)} ");
            }
            sb.Append(")");
            string atomicTotal = sb.ToString();
            //string totalCost = ("× " + (BillStack.MetalAmount[resource] * resource.BaseMarketValue * MarketPriceFactor));
            //string marketValue = " (" + (resource.BaseMarketValue + " ").Colorize(Color.yellow);
            //string multiplier = ("* " + MarketPriceFactor).Colorize(TRColor.Orange) + ")";

            Vector2 label0Size = Text.CalcSize(atomicTotal);

            //Vector2 label1Size = Text.CalcSize(totalCost);
            //Vector2 label2Size = Text.CalcSize(marketValue);
            //Vector2 label3Size = Text.CalcSize(multiplier);

            Rect atomicTotalRect = new Rect(pos.x, pos.y, label0Size.x, label0Size.y);
            //Rect totalCostRect = new Rect(pos.x, pos.y, label1Size.x, label1Size.y);
            //Rect baseMarketValueRect = new Rect(totalCostRect.xMax, pos.y, label2Size.x, label2Size.y);
            //Rect multiplierRect = new Rect(baseMarketValueRect.xMax, pos.y, label3Size.x, label3Size.y);

            Widgets.Label(atomicTotalRect, atomicTotal);
            //Widgets.Label(totalCostRect, totalCost);
            //Widgets.Label(baseMarketValueRect, marketValue);
            //Widgets.Label(multiplierRect, multiplier);
            //TooltipHandler.TipRegion(totalCostRect, "");
            //TooltipHandler.TipRegion(baseMarketValueRect,  resource.LabelCap + "'s market value.");
            //TooltipHandler.TipRegion(multiplierRect, "Tiberium Cost Factor");
        }
    }
}
