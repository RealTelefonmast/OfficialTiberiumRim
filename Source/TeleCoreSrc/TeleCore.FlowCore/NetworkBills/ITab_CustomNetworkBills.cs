using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;

namespace TeleCore
{
    public class ITab_CustomNetworkBills : ITab
    {
        private static readonly Vector2 WinSize = new Vector2(800, 500);
        private static readonly float resourceSize = 26;

        private static float maxLabelWidth = 0;

        //Scrollers
        private Vector2 billCreationResourceScroller = new Vector2();
        private Vector2 billReadourScroller = new Vector2();

        //Cachery
        private bool inputDirty = false;
        private DefValue<NetworkValueDef, float>[] cachedCustomCost;

        public Comp_NetworkBillsCrafter CrafterComp => SelThing.TryGetComp<Comp_NetworkBillsCrafter>();
        public NetworkBillStack BillStack => CrafterComp.BillStack;

        public List<CustomRecipeRatioDef> Ratios => CrafterComp.Props.UsedRatioDefs;
        public List<CustomRecipePresetDef> Presets => CrafterComp.Props.UsedPresetDefs;
        public CustomNetworkBill ClipBoard => ClipBoardUtility.TryGetClipBoard<CustomNetworkBill>(StringCache.NetworkBillClipBoard);

        private CustomBillTab SelTab { get; set; }

        private enum CustomBillTab
        {
            PresetBills,
            CustomBills
        }

        public ITab_CustomNetworkBills()
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
            ClipBoardUtility.TrySetClipBoard<CustomNetworkBill>(StringCache.NetworkBillClipBoard, null);
        }

        //Drawing
        public override void FillTab()
        {
            Text.Font = GameFont.Small;
            Rect mainRect = new Rect(0, 24, WinSize.x, WinSize.y - 24).ContractedBy(10);
            Rect leftPart = mainRect.LeftPart(0.6f);
            Rect rightPart = mainRect.RightPart(0.4f);

            var leftArea = leftPart.ContractedBy(5);
            Rect tabRect = new Rect(leftArea.x, leftArea.y, leftArea.width, 32);
            Rect contentRect = new Rect(leftArea.x, leftArea.y, leftArea.width, leftArea.height);

            Rect pasteButton = new Rect(rightPart.x, rightPart.y - 22, 22, 22);

            //Left Part
            //Draw Tabs
            var tabs = new List<TabRecord>();
            tabs.Add(new TabRecord("TR_CustomBillPresetBills".Translate(), delegate { SelTab = CustomBillTab.PresetBills; }, SelTab == CustomBillTab.PresetBills));
            tabs.Add(new TabRecord("TR_CustomBillCustom".Translate(), delegate { SelTab = CustomBillTab.CustomBills; }, SelTab == CustomBillTab.CustomBills));
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
                if (Widgets.ButtonImage(pasteButton, TeleContent.Paste))
                {
                    BillStack.PasteFromClipBoard(clipBoardValue);
                }
            }
            else
            {
                GUI.color = Color.gray;
                Widgets.DrawTextureFitted(pasteButton, TeleContent.Paste, 1);
                GUI.color = Color.white;
            }

            //Draw Details
            Rect detailRect = new Rect(TabRect.xMax, TabRect.y, 200, WinSize.y);
            BillStack.TryDrawBillDetails(detailRect);
        }

        private void DrawBillReadout(Rect inRect)
        {
            var readoutRect = inRect;

            Widgets.DrawMenuSection(inRect);
            Rect viewRect = new Rect(inRect.x, inRect.y, readoutRect.width, CrafterComp.billStack.Bills.Sum(a => a.DrawHeight));
            Widgets.BeginScrollView(readoutRect, ref billReadourScroller, viewRect, false);
            {
                float curY = inRect.y;
                for (var index = 0; index < CrafterComp.billStack.Count; index++)
                {
                    var bill = CrafterComp.billStack[index];
                    var billRect = new Rect(inRect.x, curY, readoutRect.width, bill.DrawHeight);
                    bill.DrawBill(billRect, index);
                    if (bill == BillStack.CurrentBill)
                    {
                        TWidgets.DrawBox(billRect, TColor.White05, 2);
                    }
                    curY += bill.DrawHeight;
                }
            }
            Widgets.EndScrollView();
        }

        private Vector2 presetScrollVec = Vector2.zero;

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

            float resultListHeight = ((24 + 5) * presetDef.Results.Count);
            float labelHeight = labelSize.y * 2;
            uiSizes.Register(1, (labelHeight > resultListHeight ? labelHeight : resultListHeight)); //Content Size
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
                WidgetRow row = new WidgetRow(0, curY, UIDirection.RightThenDown);
                row.Icon(result.ThingDef.uiIcon, result.ThingDef.description);
                row.Label($"×{result.Count}");
                curY += 24 + 5;
            }
            Widgets.EndGroup();

            //Draw Cost
            var costLabelRect = new Rect(drawRect.x, contentRect.yMax + 5, contentRect.width, sizes[2]);
            Widgets.Label(costLabelRect, $"Cost: {presetDef.CostLabel}");

            TWidgets.DrawBoxHighlightIfMouseOver(rect);
            if (Widgets.ButtonInvisible(rect))
            {
                BillStack.CreateBillFromDef(presetDef);
            }
        }

        //Custom Tab
        private void BillCreation(Rect rect)
        {
            Rect topPart = rect.TopPart(0.65f);
            Rect bottomPart = rect.BottomPart(0.35f);

            //TOP PART
            topPart = topPart.ContractedBy(5f);
            Widgets.BeginGroup(topPart);

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
            Rect scrollRect = new Rect(0, label1H + 5, rect.width,
                BillStack.RequestedAmount.Count * (resourceSize + 4));

            Widgets.BeginScrollView(resourceRect, ref billCreationResourceScroller, scrollRect, false);
            float curY = label1H + 5;
            for (int i = 0; i < Ratios.Count; i++)
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
            string workLabel = "Work To Do: " + BillStack.TotalWorkAmount;
            string tiberiumCostLabel = $"Cost: {NetworkBillUtility.CostLabel(TryGetCachedCost())}";
            Vector2 nameLabelSize = Text.CalcSize(nameLabel);
            Vector2 workLabelSize = Text.CalcSize(workLabel);
            Vector2 tiberiumCostLabelSize = Text.CalcSize(tiberiumCostLabel);
            Rect nameLabelRect = new Rect(0, 0, nameLabelSize.x, nameLabelSize.y);
            Rect nameFieldRect = new Rect(nameLabelRect.xMax, 0, (rect.width / 2) - nameLabelRect.width,
                nameLabelRect.height);

            Rect workLabelRect = new Rect(0, nameLabelRect.yMax + 5, workLabelSize.x, workLabelSize.y);
            Rect tiberiumCostLabelRect =
                new Rect(0, workLabelRect.yMax, tiberiumCostLabelSize.x, tiberiumCostLabelSize.y);
            Rect addButtonRect = new Rect(rect.width - 80, rect.height - 30, 80, 30);

            Widgets.Label(nameLabelRect, nameLabel);
            BillStack.billName = Widgets.TextField(nameFieldRect, BillStack.billName);

            Widgets.Label(workLabelRect, workLabel);
            Widgets.Label(tiberiumCostLabelRect, tiberiumCostLabel);

            if (Widgets.ButtonText(addButtonRect, "TR_AddBill".Translate()))
            {
                BillStack.TryCreateNewBill();
            }

            Widgets.EndGroup();
        }

        private void ResourceRow(Rect rect, CustomRecipeRatioDef recipeRatio, int index)
        {
            var resource = recipeRatio.result;
            Rect iconRect = new Rect(rect.xMin, rect.y, resourceSize, resourceSize);
            Vector2 labelSize = Text.CalcSize(resource.LabelCap);
            if (labelSize.x > maxLabelWidth) maxLabelWidth = labelSize.x;

            Rect labelRect = new Rect(iconRect.xMax, rect.y, labelSize.x, resourceSize);
            Rect fieldRect = new Rect(iconRect.xMax + maxLabelWidth + 5, rect.y, 60, resourceSize);

            Widgets.ThingIcon(iconRect, resource);
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(labelRect, resource.LabelCap);
            Text.Anchor = default;

            int temp, compare = temp = BillStack.RequestedAmount[recipeRatio];
            Widgets.TextFieldNumeric<int>(fieldRect, ref temp, ref BillStack.textBuffers[index], 0, resource.stackLimit * 2); //(int)Widgets.HorizontalSlider(sliderRect, MetalAmount[resource], 0, 100, false, default, default, default, 1);
            BillStack.RequestedAmount[recipeRatio] = temp;
            if (compare != temp)
            {
                inputDirty = true;
            }

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

        public DefValue<NetworkValueDef, float>[] TryGetCachedCost()
        {
            if (!inputDirty && cachedCustomCost != null) return cachedCustomCost;
            inputDirty = false;
            BillStack.TotalCost = cachedCustomCost = NetworkBillUtility.ConstructCustomCost(BillStack.RequestedAmount);

            return cachedCustomCost;
        }

        //
        private void CostLabel(Vector2 pos, CustomRecipeRatioDef recipeRatio)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(" x (");
            for (var i = 0; i < recipeRatio.inputRatio.Count; i++)
            {
                var input = recipeRatio.inputRatio[i];
                sb.Append($"{input.value}{input.def.labelShort.Colorize(input.def.valueColor)}");
                if (i + 1 < recipeRatio.inputRatio.Count)
                    sb.Append(" ");
            }
            sb.Append(")");
            string atomicTotal = sb.ToString();
            Vector2 label0Size = Text.CalcSize(atomicTotal);
            Rect atomicTotalRect = new Rect(pos.x, pos.y, label0Size.x, label0Size.y);
            Widgets.Label(atomicTotalRect, atomicTotal);
        }
    }
}
