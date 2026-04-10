using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using TeleCore.Shared;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace TeleCore
{
    public class CustomNetworkBill : IExposable
    {
        private static NetworkRole NetworkFlags => NetworkRole.Storage | NetworkRole.Producer;

        //General
        public NetworkBillStack billStack;

        //Custom
        public string billName;
        public float workAmountTotal;
        public DefValue<NetworkValueDef, float>[] networkCost;
        public List<ThingDefCount> results = new List<ThingDefCount>();

        private BillRepeatModeDef repeatMode = BillRepeatModeDefOf.Forever;
        private BillStoreModeDef storeMode = BillStoreModeDefOf.BestStockpile;
        private Zone_Stockpile storeZone;
        public Zone_Stockpile includeFromZone;

        public int targetCount = 1;
        public int repeatCount = -1;
        private float workAmountLeft;
        private bool hasBeenPaid = false;

        //
        private List<DefValue<NetworkValueDef, float>> scribedListInt;

        private static float borderWidth = 5;
        private static float contentHeight = 0;

        public Map Map => billStack.ParentBuilding.Map;
        public float WorkLeft => workAmountLeft;

        private bool HasBeenPaid => hasBeenPaid;
        private bool CanBeWorkedOn => hasBeenPaid || (CanPay());

        private string WorkLabel => "TELE.NetworkBill.WorkLabel".Translate((int)workAmountLeft);
        private string CostLabel => "TELE.NetworkBill.CostLabel".Translate(NetworkBillUtility.CostLabel(networkCost));

        public int CurrentCount => CustomNetworkBillUtility.CountProducts(this);

        private string CountLabel
        {
            get
            {
                if(repeatMode == BillRepeatModeDefOf.Forever)
                    return "Forever.";
                if (repeatMode == BillRepeatModeDefOf.RepeatCount)
                    return $"{repeatCount}x";
                if (repeatMode == BillRepeatModeDefOf.TargetCount)
                    return $"{CurrentCount}/{targetCount}x";
                return "Something is broken :(";
            }
        }

        public BillRepeatModeDef RepeatMode => repeatMode;
        public BillStoreModeDef StoreMode => storeMode;

        //
        public Zone_Stockpile StoreZone => storeZone;

        public float DrawHeight
        {
            get
            {
                float height = 0;
                var labelSize = Text.CalcSize(billName);
                height += labelSize.y;

                float resultListHeight = ((24 + 5) * results.Count);
                float labelHeight = labelSize.y * 2;
                height += (contentHeight = (labelHeight > resultListHeight ? labelHeight : resultListHeight));
                height += (borderWidth * 2) + 30;
                return height;
            }
        }

        public void ExposeData()
        {
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                scribedListInt = networkCost.ToList();
            }

            Scribe_Values.Look(ref billName, "billName");
            Scribe_Values.Look(ref repeatCount, "iterationsLeft");
            Scribe_Values.Look(ref workAmountTotal, "workAmountTotal");
            Scribe_Values.Look(ref workAmountLeft, "workAmountLeft");
            Scribe_Values.Look(ref hasBeenPaid, "hasBeenPaid");
            Scribe_Collections.Look(ref results, "results");

            Scribe_Collections.Look(ref scribedListInt, "networkCostList", LookMode.Deep);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                networkCost = scribedListInt.ToArray();
            }
        }

        public CustomNetworkBill(NetworkBillStack stack)
        {
            this.billStack = stack;
        }

        public CustomNetworkBill(float workAmount)
        {
            workAmountTotal = workAmountLeft = workAmount;
        }

        public bool ShouldDoNow()
        {
            if (!CanBeWorkedOn) return false;
            if (repeatMode == BillRepeatModeDefOf.RepeatCount && repeatCount == 0) return false;
            if (repeatMode == BillRepeatModeDefOf.TargetCount && CurrentCount >= targetCount) return false;
            return true;
        }

        private bool CanPay()
        {
            if (networkCost.NullOrEmpty())
            {
                TLog.Error($"Trying to pay for {billName} with empty networkCost! | Paid: {HasBeenPaid} WorkLeft: {WorkLeft}");
                return false;
            }

            float totalNeeded = networkCost.Sum(t => t.Value);
            foreach (var value in networkCost)
            {
                var network = billStack.ParentComp[value.Def.networkDef].Network;
                if (network.NetworkValueFor(value.Def, NetworkFlags) >= value.Value)
                {
                    totalNeeded -= value.Value;
                }
            }
            return totalNeeded == 0f;
        }

        public bool TryFinish(out List<Thing> products)
        {
            products = null;
            if (workAmountLeft > 0) return false;

            products = new List<Thing>();
            foreach (var defCount in results)
            {
                int desiredAmount = defCount.Count;
                while (desiredAmount > 0)
                {
                    int possibleAmount = Mathf.Clamp(desiredAmount, 0, defCount.ThingDef.stackLimit);
                    Thing thing = ThingMaker.MakeThing(defCount.ThingDef);
                    products.Add(thing);

                    thing.stackCount = possibleAmount;
                    GenPlace.TryPlaceThing(thing, billStack.ParentBuilding.InteractionCell, billStack.ParentBuilding.Map, ThingPlaceMode.Near);
                    desiredAmount -= possibleAmount;
                }

                if (repeatCount > 0)
                    repeatCount--;

                if (repeatCount is -1 or > 0)
                    Reset();

                if (repeatCount == 0)
                    billStack.Delete(this);
            }
            return true;
        }

        private void Reset()
        {
            workAmountLeft = workAmountTotal;
            hasBeenPaid = false;
        }

        //Allocate network cost as "paid", refund if cancelled
        public void DoWork(Pawn pawn)
        {
            StartWorkAndPay();
            float num = pawn.GetStatValue(StatDefOf.GeneralLaborSpeed, true);
            Building billBuilding = billStack.ParentBuilding;
            if (billBuilding != null)
            {
                num *= billBuilding.GetStatValue(StatDefOf.WorkSpeedGlobal, true);
            }

            if (DebugSettings.fastCrafting)
            {
                num *= 30f;
            }
            workAmountLeft = Mathf.Clamp(workAmountLeft - num, 0, float.MaxValue);
        }

        private void StartWorkAndPay()
        {
            if (HasBeenPaid) return;
            if (TryPay()) return;

            //Failed to pay...
        }

        private bool TryPay()
        {
            var storages = billStack.ParentNetComps.SelectMany(n => n.ContainerSet[NetworkFlags]);
            NetworkValueStack stack = new NetworkValueStack();
            foreach (var value in networkCost)
            {
                stack.Add(value.Def, value.Value);
            }

            foreach (var storage in storages)
            {
                foreach (var value in stack.networkValues)
                {
                    if (storage.ValueForType(value.valueDef) > 0 && storage.TryRemoveValue(value.valueDef, value.valueF, out float actualVal))
                    {
                        stack -= new NetworkValue(value.valueDef, actualVal);
                    }

                    if (stack.TotalValue <= 0)
                    {
                        hasBeenPaid = true;
                        return true;
                    }
                }
            }

            if (stack.TotalValue > 0)
                TLog.Error($"TotalCost higher than 0 after payment! LeftOver: {stack.TotalValue}");
            return false;
        }

        //Refund
        public void Cancel()
        {
            if (HasBeenPaid)
                Refund();
        }

        private NetworkValueStack StackFor(NetworkComponent comp)
        {
            var storages = comp.ContainerSet[NetworkFlags];
            NetworkValueStack stack = new NetworkValueStack();
            foreach (var value in networkCost)
            {
                if(value.Def.networkDef == comp.NetworkDef)
                    stack.Add(value.Def, value.Value);
            }

            foreach (var storage in storages)
            {
                foreach (var value in stack.networkValues)
                {
                    if (storage.TryAddValue(value.valueDef, value.valueF, out float actualValue))
                    {
                        stack -= new NetworkValue(value.valueDef, actualValue);
                    }
                }
            }
            return stack;
        }

        private void Refund()
        {
            foreach (var netComp in billStack.ParentNetComps)
            {
                var portableDef = netComp.NetworkDef.portableContainerDef;
                if (portableDef == null) continue;
                var newStack = StackFor(netComp);
                if (newStack.TotalValue > 0)
                {
                    TLog.Warning($"Stack not empty ({newStack.TotalValue}) after refunding... dropping container.");
                    PortableContainer container = (PortableContainer)ThingMaker.MakeThing(portableDef);
                    container.SetupProperties(netComp.NetworkDef, new NetworkContainer(container, newStack),new ContainerProperties()
                    {
                        maxStorage = Mathf.RoundToInt(newStack.TotalValue)
                    });
                    //
                    container.Container.UpdateContainerState(true);
                    GenPlace.TryPlaceThing(container, billStack.ParentBuilding.Position, billStack.ParentBuilding.Map, ThingPlaceMode.Near);
                }
            }
        }

        public void DrawBill(Rect rect, int index)
        {
            if (RepeatMode == BillRepeatModeDefOf.TargetCount && CurrentCount > targetCount)
            {
                TWidgets.DrawHighlightColor(rect, TColor.Orange);
            }

            if (!CanBeWorkedOn)
            {
                TWidgets.DrawHighlightColor(rect, Color.red);
            }

            if (HasBeenPaid)
            {
                TWidgets.DrawHighlightColor(rect, Color.green);
            }

            if (index % 2 == 0)
                Widgets.DrawAltRect(rect);
            rect = rect.ContractedBy(5);

            Widgets.BeginGroup(rect);
            {
                rect = rect.AtZero();

                //Name
                Vector2 labelSize = Text.CalcSize(billName);
                Rect labelRect = new Rect(new Vector2(0, 0), labelSize);
                Widgets.Label(labelRect, billName);

                //Controls
                Rect removeRect = new Rect(rect.width - 20f, 0f, 22f, 22f);
                Rect copyRect = new Rect(removeRect.x - 20, 0f, 22f, 22f);
                if (Widgets.ButtonImage(removeRect, TexButton.DeleteX, Color.white, Color.white * GenUI.SubtleMouseoverColor, true))
                {
                    billStack.Delete(this);
                }
                if (Widgets.ButtonImageFitted(copyRect, TeleContent.Copy, Color.white))
                {
                    ClipBoardUtility.TrySetClipBoard(RWStringCache.NetworkBillClipBoard, Clone());
                    SoundDefOf.Tick_High.PlayOneShotOnCamera(null);
                }

                var newRect = new Rect(0, labelRect.height, rect.width, contentHeight);
                var leftRect = newRect.LeftHalf();
                var rightRect = newRect.RightHalf();

                //LEFT
                Widgets.BeginGroup(leftRect);
                {
                    //List
                    float curY = 0;
                    foreach (var result in results)
                    {
                        WidgetRow row = new WidgetRow(0, curY, UIDirection.RightThenDown);
                        row.Icon(result.ThingDef.uiIcon, result.ThingDef.description);
                        row.Label($"×{result.Count}");
                        curY += 24 + 5;
                    }
                }
                Widgets.EndGroup();

                //RIGHT
                Widgets.BeginGroup(rightRect);
                {
                    Rect workBarRect = new Rect(rightRect.width - 75, rightRect.height - (24 + 5), 100, 24);
                    Widgets.FillableBar(workBarRect, Mathf.InverseLerp(0, workAmountTotal, workAmountTotal - workAmountLeft));
                }
                Widgets.EndGroup();

                Rect bottomRect = new Rect(0, newRect.yMax, rect.width, 24);
                Widgets.BeginGroup(bottomRect);
                {
                    bottomRect = bottomRect.AtZero();

                    Vector2 countLabelSize = Text.CalcSize(CountLabel);
                    Rect countLabelRect = new Rect(0, 0, countLabelSize.x, countLabelSize.y);
                    Widgets.Label(countLabelRect, CountLabel);

                    WidgetRow controlRow = new WidgetRow();
                    controlRow.Init(bottomRect.xMax, 0, UIDirection.LeftThenUp);
                    if (controlRow.ButtonText("Details".Translate() + "..."))
                    {
                        billStack.RequestDetails(this);
                    }
                    if (controlRow.ButtonText(repeatMode.LabelCap))
                    {
                        DoRepeatModeConfig();
                    }

                    if (repeatMode == BillRepeatModeDefOf.RepeatCount)
                    {
                        int incrementor = 1;
                        if (Input.GetKey(KeyCode.LeftShift))
                            incrementor = 10;
                        if (Input.GetKey(KeyCode.LeftControl))
                            incrementor = 100;

                        //
                        if (controlRow.ButtonIcon(TeleContent.Plus))
                        {
                            repeatCount += incrementor;
                        }
                        if (controlRow.ButtonIcon(TeleContent.Minus))
                        {
                            repeatCount = Mathf.Clamp(repeatCount - incrementor, 0, int.MaxValue);
                        }
                    }

                    if (repeatMode == BillRepeatModeDefOf.TargetCount)
                    {
                        int incrementor = 1;
                        if (Input.GetKey(KeyCode.LeftShift))
                            incrementor = 10;
                        if (Input.GetKey(KeyCode.LeftControl))
                            incrementor = 100;

                        //
                        if (controlRow.ButtonIcon(TeleContent.Plus))
                        {
                            targetCount += incrementor;
                        }
                        if (controlRow.ButtonIcon(TeleContent.Minus))
                        {
                            targetCount = Mathf.Clamp(targetCount - incrementor, 0, int.MaxValue);
                        }
                    }
                }
                Widgets.EndGroup();
            }
            Widgets.EndGroup();
        }

        public CustomNetworkBill Clone()
        {
            CustomNetworkBill bill = new CustomNetworkBill(workAmountTotal);
            bill.repeatCount = repeatCount;
            bill.billName = billName + "_Copy";
            bill.repeatMode = repeatMode;
            bill.networkCost = new DefValue<NetworkValueDef, float>[networkCost.Length];
            networkCost.CopyTo(bill.networkCost);
            bill.results = new List<ThingDefCount>(results);
            return bill;
        }

        public void DoRepeatModeConfig()
        {
            var list = new List<FloatMenuOption>();
            list.Add(new FloatMenuOption(BillRepeatModeDefOf.RepeatCount.LabelCap, delegate { repeatMode = BillRepeatModeDefOf.RepeatCount; }));
            list.Add(new FloatMenuOption(BillRepeatModeDefOf.TargetCount.LabelCap, delegate
            {
                /*
                if (!recipe.WorkerCounter.CanCountProducts(bill))
                {
                    Messages.Message("RecipeCannotHaveTargetCount".Translate(), MessageTypeDefOf.RejectInput, false);
                    return;
                }
                */
                repeatMode = BillRepeatModeDefOf.TargetCount;
            }));
            list.Add(new FloatMenuOption(BillRepeatModeDefOf.Forever.LabelCap, delegate { repeatMode = BillRepeatModeDefOf.Forever; }));
            Find.WindowStack.Add(new FloatMenu(list));
        }

        public void DoStoreModeConfig()
        {
            Text.Font = GameFont.Small;
            List<FloatMenuOption> list = new List<FloatMenuOption>();
            foreach (BillStoreModeDef billStoreModeDef in from bsm in DefDatabase<BillStoreModeDef>.AllDefs orderby bsm.listOrder select bsm)
            {
                if (billStoreModeDef == BillStoreModeDefOf.SpecificStockpile)
                {
                    List<SlotGroup> allGroupsListInPriorityOrder = billStack.ParentBuilding.Map.haulDestinationManager.AllGroupsListInPriorityOrder;
                    int count = allGroupsListInPriorityOrder.Count;
                    for (int i = 0; i < count; i++)
                    {
                        SlotGroup group = allGroupsListInPriorityOrder[i];
                        if (group.parent is Zone_Stockpile stockpile)
                        {
                            //!bill.recipe.WorkerCounter.CanPossiblyStoreInStockpile(this.bill, stockpile)
                            if (!CanPossiblyStoreInStockpile(stockpile))
                            {
                                list.Add(new FloatMenuOption($"{string.Format(billStoreModeDef.LabelCap, @group.parent.SlotYielderLabel())} ({"IncompatibleLower".Translate()})", null));
                            }
                            else
                            {
                                list.Add(new FloatMenuOption(string.Format(billStoreModeDef.LabelCap, group.parent.SlotYielderLabel()), delegate ()
                                {
                                    SetStoreMode(BillStoreModeDefOf.SpecificStockpile, (Zone_Stockpile)group.parent);
                                }));
                            }
                        }
                    }
                }
                else
                {
                    list.Add(new FloatMenuOption(billStoreModeDef.LabelCap, delegate ()
                    {
                       SetStoreMode(billStoreModeDef, null);
                    }));
                }
            }
            Find.WindowStack.Add(new FloatMenu(list));
        }

        public void SetStoreMode(BillStoreModeDef mode, Zone_Stockpile zone = null)
        {
            this.storeMode = mode;
            this.storeZone = zone;
            if (this.storeMode == BillStoreModeDefOf.SpecificStockpile != (this.storeZone != null))
            {
                Log.ErrorOnce("Inconsistent bill StoreMode data set", 75645354);
            }
        }

        public bool CanPossiblyStoreInStockpile(Zone_Stockpile stockpile)
        {
           return stockpile.GetStoreSettings().AllowedToAccept(results[0].ThingDef);
        }
    }
}
