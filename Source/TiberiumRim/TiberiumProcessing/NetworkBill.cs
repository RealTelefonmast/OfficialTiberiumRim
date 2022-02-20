using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace TiberiumRim
{
    //TODO: Potentially make this a "NetworkBill"
    public class CustomNetworkBill : IExposable
    {
        //General
        public NetworkBillStack billStack;

        //Custom
        public string billName;
        public int iterationsLeft = -1;
        public float workAmountTotal;
        public DefValue<NetworkValueDef>[] networkCost;
        public List<ThingDefCount> results = new List<ThingDefCount>();

        private BillRepeatModeDef repeatMode = BillRepeatModeDefOf.Forever;
        private float workAmountLeft;

        private static float borderWidth = 5;
        private static float contentHeight = 0;

        public float WorkLeft => workAmountLeft;

        private string WorkLabel => "TR_NetworkBillWork".Translate((int)workAmountLeft);
        private string CostLabel => "TR_NetworkBillCost".Translate(ITab_CustomRefineryBills.CostLabel(networkCost));

        private string CountLabel
        {
            get
            {
                if (iterationsLeft == -1)
                    return "Forever.";
                if (iterationsLeft >= 0)
                    return $"{iterationsLeft}x";
                return "Something is broken :(";
            }
        }

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

        private Type type;

        public void ExposeData()
        {
            Scribe_Values.Look(ref billName, "billName");
            Scribe_Values.Look(ref iterationsLeft, "iterationsLeft");
            Scribe_Universal.Look(ref networkCost, "networkCost", LookMode.Deep, ref type); 
            Scribe_Values.Look(ref workAmountTotal, "workAmountTotal");
            Scribe_Values.Look(ref workAmountLeft, "workAmountLeft");
            Scribe_Collections.Look(ref results, "results");
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
            if (!CanPay()) return false;
            if (iterationsLeft == 0) return false;

            return true;
        }

        private bool CanPayWith(Network wholeNetwork)
        {
            var totalNetworkValue = wholeNetwork.TotalNetworkValue;
            float totalNeeded = networkCost.Sum(t => t.Value);
            if (totalNetworkValue < totalNeeded) return false;
     
                foreach (var typeCost in networkCost)
                {
                    var specCost = typeCost.Value;
                    if (wholeNetwork.NetworkValueFor(typeCost.Def) >= specCost)
                        totalNeeded -= specCost;
                }
            


            return totalNeeded == 0;
        }

        private bool CanPay()
        {
            float totalNeeded = networkCost.Sum(t => t.Value);
            foreach (var value in networkCost)
            {
                var network = billStack.ParentComp[value.Def.networkDef].Network;
                if (network.NetworkValueFor(value.Def) >= value.Value)
                {
                    totalNeeded -= value.Value;
                }
            }

            return totalNeeded == 0;
        }

        public void Pay()
        {
            var network = billStack.ParentTibComp.Network;
            float totalNeeded = networkCost.Sum(t => t.Value);
            var storages = network.ComponentSet.Storages;
            foreach (var storage in storages)
            {
                foreach (var value in networkCost)
                {
                    if (totalNeeded <= 0) return;
                    if (storage.Container.ValueForType(value.Def) > 0 && storage.Container.TryRemoveValue(value.Def, value.Value, out float actualVal))
                    {
                        totalNeeded -= actualVal;
                    }
                }
            }
            if(totalNeeded > 0)
                TLog.Error("TotalCost higher than 0 after payment!");
        }

        public bool TryFinish()
        {
            if (workAmountLeft > 0) return false;
            foreach (var defCount in results)
            {
                int desiredAmount = defCount.Count;
                while (desiredAmount > 0)
                {
                    int possibleAmount = Mathf.Clamp(desiredAmount, 0, defCount.ThingDef.stackLimit);
                    Thing thing = ThingMaker.MakeThing(defCount.ThingDef);
                    thing.stackCount = possibleAmount;
                    GenSpawn.Spawn(thing, billStack.ParentBuilding.InteractionCell, billStack.ParentBuilding.Map, WipeMode.VanishOrMoveAside);
                    desiredAmount -= possibleAmount;

                }

                if (iterationsLeft > 0)
                    iterationsLeft--;

                if (iterationsLeft == -1 || iterationsLeft > 0)
                    Reset();

                if (iterationsLeft == 0)
                    billStack.Delete(this);
            }


            return true;
        }

        private void Reset()
        {
            workAmountLeft = workAmountTotal;
        }

        public void DoWork(Pawn pawn)
        {
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

        public void DrawBill(Rect rect, int index)
        {
            if(index % 2 == 0)
                Widgets.DrawAltRect(rect);
            rect = rect.ContractedBy(5);

            GUI.BeginGroup(rect);
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
            if (Widgets.ButtonImageFitted(copyRect, TiberiumContent.Copy, Color.white))
            {
                ClipBoardUtility.Clipboard = this.Clone();
                SoundDefOf.Tick_High.PlayOneShotOnCamera(null);
            }

            var newRect = new Rect(0, labelRect.height, rect.width, contentHeight);
            var leftRect = newRect.LeftHalf();
            var rightRect = newRect.RightHalf();

            //LEFT
            GUI.BeginGroup(leftRect);
            //List
            float curY = 0;
            foreach (var result in results) 
            {
                WidgetRow row = new WidgetRow(0, curY, UIDirection.RightThenDown);
                row.Icon(result.ThingDef.uiIcon, result.ThingDef.description);
                row.Label($"×{result.Count}");
                curY += 24 + 5;
            }
            GUI.EndGroup();

            //RIGHT
            GUI.BeginGroup(rightRect);
            Vector2 workLabelSize = Text.CalcSize(WorkLabel);
            Vector2 costLabelSize = Text.CalcSize(CostLabel);
            Rect workRect = new Rect(0,0, workLabelSize.x, workLabelSize.y);
            Rect costRect = new Rect(0, workRect.yMax, costLabelSize.x, costLabelSize.y);

            Widgets.Label(workRect, WorkLabel);
            Widgets.Label(costRect, CostLabel);

            GUI.EndGroup();

            Rect bottomRect = new Rect(0, newRect.yMax, rect.width, 24);
            GUI.BeginGroup(bottomRect);
            bottomRect = bottomRect.AtZero();

            Vector2 countLabelSize = Text.CalcSize(CountLabel);
            Rect countLabelRect = new Rect(0, 0, countLabelSize.x, countLabelSize.y);
            Widgets.Label(countLabelRect, CountLabel);

            WidgetRow controlRow = new WidgetRow(bottomRect.xMax, 0, UIDirection.LeftThenUp);
            if (controlRow.ButtonText(repeatMode.LabelCap))
            {
                DoConfigFloatMenu();
            }

            if (repeatMode == BillRepeatModeDefOf.RepeatCount)
            {
                if (controlRow.ButtonIcon(TiberiumContent.Plus))
                {
                    iterationsLeft++;
                }
                if (controlRow.ButtonIcon(TiberiumContent.Minus))
                {
                    iterationsLeft = Mathf.Clamp(iterationsLeft - 1, 0, int.MaxValue);
                }
            }
            GUI.EndGroup();
            GUI.EndGroup();
        }

        public CustomNetworkBill Clone()
        {
            CustomNetworkBill bill = new CustomNetworkBill(workAmountTotal);
            bill.iterationsLeft = iterationsLeft;
            bill.billName = billName + "_Copy";
            bill.repeatMode = repeatMode;
            networkCost.CopyTo(bill.networkCost);
            bill.results = new List<ThingDefCount>(results);
            return bill;
        }

        private void DoConfigFloatMenu()
        {
            List<FloatMenuOption> list = new List<FloatMenuOption>();
            list.Add(new FloatMenuOption(BillRepeatModeDefOf.Forever.LabelCap, delegate
            {
                this.repeatMode = BillRepeatModeDefOf.Forever;
                iterationsLeft = -1;
            }));
            list.Add(new FloatMenuOption(BillRepeatModeDefOf.RepeatCount.LabelCap, delegate
            {
                this.repeatMode = BillRepeatModeDefOf.RepeatCount;
                iterationsLeft = 1;
            }));
            /*
            list.Add(new FloatMenuOption(BillRepeatModeDefOf.TargetCount.LabelCap, delegate
            {
                this.repeatMode = BillRepeatModeDefOf.TargetCount;
            }));
            */
            Find.WindowStack.Add(new FloatMenu(list));
        }
    }

    public class NetworkBill : Bill_Production
    {
        public TRecipeDef def;
        public bool isBeingDone = false;

        public NetworkBill(TRecipeDef def) : base(def as RecipeDef)
        {
            this.def = def;
        }

        public NetworkBill() : base() { }

        public Comp_NetworkStructureCrafter CompTNW => ((Building) billStack.billGiver).GetComp<Comp_NetworkStructureCrafter>();
        //public NetworkComponent ParentTibComp => CompTNW[TiberiumDefOf.TiberiumNetwork];
        //private Network Network => ParentTibComp.Network;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref isBeingDone, "isBeingDone");
            Scribe_Defs.Look(ref def, "props");
        }       

        public override void Notify_DoBillStarted(Pawn billDoer)
        {
            base.Notify_DoBillStarted(billDoer);
            isBeingDone = true;
        }

        public override void Notify_PawnDidWork(Pawn p)
        {
            //Log.Message("Notify Pawn Did Work");
            base.Notify_PawnDidWork(p);
        }

        public bool BaseShouldDo => base.ShouldDoNow();

        public override bool ShouldDoNow()
        {
            if (base.ShouldDoNow())
            {
                if (CompTNW is {IsPowered: true})
                {
                    return def.networkCost.CanPayWith(CompTNW);
                    //if (Network != null && Network.IsWorking)
                }
            }
            return false;
        }

        public override void Notify_IterationCompleted(Pawn billDoer, List<Thing> ingredients)
        {
            if (def.networkCost.CanPayWith(CompTNW))
            {
                def.networkCost.DoPayWith(CompTNW);
                isBeingDone = false;
                base.Notify_IterationCompleted(billDoer, ingredients);
            }
        }

        public Color BillColor
        {
            get
            {
                Color color = Color.white;
                foreach(NetworkValueDef valueDef in def.networkCost.Cost.AcceptedValueTypes)
                {
                    color *= valueDef.valueColor;
                }
                return color;
            }
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }
}
