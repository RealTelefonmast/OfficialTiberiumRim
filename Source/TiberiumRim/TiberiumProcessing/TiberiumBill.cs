using System.Collections.Generic;
using RimWorld;
using TeleCore.Logging;
using TeleCore.Network.Bills;
using TeleCore.Utils;
using TR.GameParts.Networks.TiberiumNetwork;
using TR.Rendering.TextureContent;
using TR.Util;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace TR.TiberiumProcessing;

public class CustomTiberiumBill : IExposable
{
    private static readonly float borderWidth = 5;
    private static float contentHeight;
    public string billName;
    public TiberiumBillStack billStack;
    public int iterationsLeft = -1;

    private BillRepeatModeDef repeatMode = BillRepeatModeDefOf.Forever;
    public List<ThingDefCount> results = new();
    public int tiberiumCost;
    private float workAmountLeft;
    public float workAmountTotal;

    public CustomTiberiumBill(TiberiumBillStack stack)
    {
        billStack = stack;
    }

    public CustomTiberiumBill(float workAmount)
    {
        workAmountTotal = workAmountLeft = workAmount;
    }

    public float WorkLeft => workAmountLeft;

    private string WorkLabel => "Work Left: " + (int)workAmountLeft;
    private string CostLabel => "Cost: " + tiberiumCost;

    private string CountLabel
    {
        get
        {
            if (iterationsLeft == -1)
                return "Forever.";
            if (iterationsLeft >= 0)
                return iterationsLeft + "x";
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

            float resultListHeight = (24 + 5) * results.Count;
            var labelHeight = labelSize.y * 2;
            height += contentHeight = labelHeight > resultListHeight ? labelHeight : resultListHeight;
            height += borderWidth * 2 + 30;
            return height;
        }
    }

    public void ExposeData()
    {
        Scribe_Values.Look(ref billName, "billName");
        Scribe_Values.Look(ref iterationsLeft, "iterationsLeft");
        Scribe_Values.Look(ref tiberiumCost, "tiberiumCost");
        Scribe_Values.Look(ref workAmountTotal, "workAmountTotal");
        Scribe_Values.Look(ref workAmountLeft, "workAmountLeft");
        Scribe_Collections.Look(ref results, "results");
    }

    public bool ShouldDoNow()
    {
        if (!CanPay()) return false;
        if (iterationsLeft == 0) return false;

        return true;
    }

    private bool CanPay()
    {
        var network = billStack.ParntComp.Network;
        float networkValue = 0;
        foreach (var valueType in TRUtils.MainValueTypes)
        {
            networkValue += network.NetworkValueFor(valueType);
            if (networkValue > tiberiumCost)
                return true;
        }

        return false;
    }

    public void Pay()
    {
        var network = billStack.ParntComp.Network;
        float totalCost = tiberiumCost;
        var storages = network.NetworkSet.Storages;
        foreach (var storage in storages)
        foreach (var type in TRUtils.MainValueTypes)
        {
            if (totalCost <= 0) return;
            if (storage.Container.ValueForType(type) > 0 &&
                storage.Container.TryRemoveValue(type, totalCost, out var actualVal)) totalCost -= actualVal;
        }

        if (totalCost > 0)
            TLog.Error("TotalCost higher than 0 after payment!");
    }

    public bool TryFinish()
    {
        if (workAmountLeft > 0) return false;
        foreach (var defCount in results)
        {
            var desiredAmount = defCount.Count;
            while (desiredAmount > 0)
            {
                var possibleAmount = Mathf.Clamp(desiredAmount, 0, defCount.ThingDef.stackLimit);
                var thing = ThingMaker.MakeThing(defCount.ThingDef);
                thing.stackCount = possibleAmount;
                GenSpawn.Spawn(thing, billStack.ParentBuilding.InteractionCell, billStack.ParentBuilding.Map,
                    WipeMode.VanishOrMoveAside);
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
        var num = pawn.GetStatValue(StatDefOf.GeneralLaborSpeed);
        var billBuilding = billStack.ParentBuilding;
        if (billBuilding != null) num *= billBuilding.GetStatValue(StatDefOf.WorkSpeedGlobal);

        if (DebugSettings.fastCrafting) num *= 30f;
        workAmountLeft = Mathf.Clamp(workAmountLeft - num, 0, float.MaxValue);
    }

    public void DrawBill(Rect rect, int index)
    {
        if (index % 2 == 0)
            Widgets.DrawAltRect(rect);
        rect = rect.ContractedBy(5);

        GUI.BeginGroup(rect);
        rect = rect.AtZero();

        //Name
        var labelSize = Text.CalcSize(billName);
        var labelRect = new Rect(new Vector2(0, 0), labelSize);
        Widgets.Label(labelRect, billName);

        //Controls
        var removeRect = new Rect(rect.width - 20f, 0f, 22f, 22f);
        var copyRect = new Rect(removeRect.x - 20, 0f, 22f, 22f);
        if (Widgets.ButtonImage(removeRect, TiberiumContent.DeleteX, Color.white,
                Color.white * GenUI.SubtleMouseoverColor)) billStack.Delete(this);
        if (Widgets.ButtonImageFitted(copyRect, TiberiumContent.Copy, Color.white))
        {
            TiberiumBillUtility.Clipboard = Clone();
            SoundDefOf.Tick_High.PlayOneShotOnCamera();
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
            var row = new WidgetRow(0, curY, UIDirection.RightThenDown);
            row.Icon(result.ThingDef.uiIcon, result.ThingDef.description);
            row.Label("×" + result.Count);
            curY += 24 + 5;
        }

        GUI.EndGroup();

        //RIGHT
        GUI.BeginGroup(rightRect);
        var workLabelSize = Text.CalcSize(WorkLabel);
        var costLabelSize = Text.CalcSize(CostLabel);
        var workRect = new Rect(0, 0, workLabelSize.x, workLabelSize.y);
        var costRect = new Rect(0, workRect.yMax, costLabelSize.x, costLabelSize.y);

        Widgets.Label(workRect, WorkLabel);
        Widgets.Label(costRect, CostLabel);

        GUI.EndGroup();

        var bottomRect = new Rect(0, newRect.yMax, rect.width, 24);
        GUI.BeginGroup(bottomRect);
        bottomRect = bottomRect.AtZero();

        var countLabelSize = Text.CalcSize(CountLabel);
        var countLabelRect = new Rect(0, 0, countLabelSize.x, countLabelSize.y);
        Widgets.Label(countLabelRect, CountLabel);

        var controlRow = new WidgetRow(bottomRect.xMax, 0, UIDirection.LeftThenUp);
        if (controlRow.ButtonText(repeatMode.LabelCap)) DoConfigFloatMenu();

        if (repeatMode == BillRepeatModeDefOf.RepeatCount)
        {
            if (controlRow.ButtonIcon(TiberiumContent.Minus))
                iterationsLeft = Mathf.Clamp(iterationsLeft - 1, 0, int.MaxValue);
            if (controlRow.ButtonIcon(TiberiumContent.Plus)) iterationsLeft++;
        }

        GUI.EndGroup();
        GUI.EndGroup();
    }

    public CustomTiberiumBill Clone()
    {
        var bill = new CustomTiberiumBill(workAmountTotal);
        bill.iterationsLeft = iterationsLeft;
        bill.billName = billName + "_Copy";
        bill.repeatMode = repeatMode;
        bill.tiberiumCost = tiberiumCost;
        bill.results = new List<ThingDefCount>(results);
        return bill;
    }

    private void DoConfigFloatMenu()
    {
        var list = new List<FloatMenuOption>();
        list.Add(new FloatMenuOption(BillRepeatModeDefOf.Forever.LabelCap, delegate
        {
            repeatMode = BillRepeatModeDefOf.Forever;
            iterationsLeft = -1;
        }));
        list.Add(new FloatMenuOption(BillRepeatModeDefOf.RepeatCount.LabelCap, delegate
        {
            repeatMode = BillRepeatModeDefOf.RepeatCount;
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

public class TiberiumBill : Bill_Production
{
    public TRecipeDef def;
    public bool isBeingDone;

    public TiberiumBill(TRecipeDef def) : base(def)
    {
        this.def = def;
    }

    public TiberiumBill()
    {
    }

    private Comp_TiberiumNetworkStructure TibNetComp => ((Building)billStack.billGiver).GetComp<Comp_TiberiumNetworkStructure>();

    public Color BillColor
    {
        get
        {
            var color = Color.white;
            if (def.networkCost?.costSet != null)
                foreach (var type in def.networkCost.costSet.AcceptedValueTypes) color *= type.valueColor;
            return color;
        }
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref isBeingDone, "isBeingDone");
        Scribe_Defs.Look(ref def, "def");
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


    public override bool ShouldDoNow()
    {
        if (base.ShouldDoNow())
            if (TibNetComp != null && TibNetComp.HasConnection)
                return def.networkCost?.CanPayWith(TibNetComp.TiberiumNetPart) ?? true;

        return false;
    }

    public override void Notify_IterationCompleted(Pawn billDoer, List<Thing> ingredients)
    {
        if (def.networkCost == null || def.networkCost.CanPayWith(TibNetComp.TiberiumNetPart))
        {
            def.networkCost?.DoPayWith(TibNetComp);
            isBeingDone = false;
            base.Notify_IterationCompleted(billDoer, ingredients);
        }
    }
}