using System.Collections.Generic;
using System.Linq;
using TeleCore.FlowCore.Utility;
using TeleCore.RimWorld.Defs;
using TeleCore.Shared;
using UnityEngine;
using Verse;

namespace TeleCore.FlowCore.Bills;

public class NetworkBillStack : IExposable
{
    public static readonly float MarketPriceFactor = 2.4f;
    public static readonly float WorkAmountFactor = 10;

    //Custom Bill Creation
    public int billID = 1;
    public string billName = "";
    public Dictionary<CustomRecipeRatioDef, int> RequestedAmount = new();
    public string[] textBuffers;

    //Stack
    private List<CustomNetworkBill> bills = new();

    //Details
    private CustomNetworkBill curDetailRequester;

    public NetworkBillStack(Comp_NetworkBillsCrafter parent)
    {
        ParentComp = parent;
        textBuffers = new string[Ratios.Count];
        foreach (var recipe in Ratios)
            RequestedAmount.Add(recipe, 0);

        ResetBillData();
    }

    public DefValueStack<NetworkValueDef, float> TotalCost { get; set; }
    public DefValueStack<NetworkValueDef, float> ByProducts { get; set; }

    public int TotalWorkAmount => TotalCost.IsEmpty ? 0 : TotalCost.Values.Sum(m => (int)(m.Value * WorkAmountFactor));

    //
    public Building ParentBuilding => ParentComp.parent;
    public Comp_NetworkBillsCrafter ParentComp { get; }

    public List<CustomRecipeRatioDef> Ratios => ParentComp.Props.UsedRatioDefs;

    public List<CustomNetworkBill> Bills => bills;
    public CustomNetworkBill CurrentBill => bills.FirstOrDefault(c => c?.ShouldDoNow() ?? false);
    public int Count => bills.Count;

    public CustomNetworkBill this[int index] => bills[index];

    public void ExposeData()
    {
        Scribe_Values.Look(ref billID, "billID");
        Scribe_Values.Look(ref billName, "billName");
        Scribe_Collections.Look(ref RequestedAmount, "requestAmount");
        Scribe_Collections.Look(ref bills, "bills", LookMode.Deep, this);

        if (Scribe.mode == LoadSaveMode.PostLoadInit)
            for (var i = 0; i < RequestedAmount.Count; i++)
                textBuffers[i] = RequestedAmount.ElementAt(i).Value.ToString();
    }

    public void CreateBillFromDef(CustomRecipePresetDef presetDefDef)
    {
        var totalCost = presetDefDef.desiredResources.Sum(t => (int)(t.Value * WorkAmountFactor));
        var customBill = new CustomNetworkBill(totalCost);
        customBill.billName = presetDefDef.defName;
        customBill.SetCost(NetworkBillUtility.ConstructCustomCostStack(presetDefDef.desiredResources));
        if (presetDefDef.HasByProducts)
            customBill.byProducts = NetworkBillUtility.ConstructCustomCostStack(presetDefDef.desiredResources, true);
        customBill.AssignToStack(this);
        customBill.results = presetDefDef.Results;
        bills.Add(customBill);
    }

    public void TryCreateNewBill()
    {
        if (TotalCost.IsEmpty) return;

        var customBill = new CustomNetworkBill(TotalWorkAmount);
        customBill.billName = billName;
        customBill.SetCost(new DefValueStack<NetworkValueDef, float>(TotalCost));

        if (!ByProducts.IsEmpty)
            customBill.byProducts = new DefValueStack<NetworkValueDef, float>(ByProducts);

        customBill.AssignToStack(this);
        customBill.results = RequestedAmount.Where(m => m.Value > 0)
            .Select(m => new ThingDefCount(m.Key.result, m.Value)).ToList();
        bills.Add(customBill);
        billID++;

        //Clear Data
        ResetBillData();
    }

    public void PasteFromClipBoard(CustomNetworkBill clipBoardVal)
    {
        clipBoardVal.AssignToStack(this);
        bills.Add(clipBoardVal);
    }

    public void Delete(CustomNetworkBill bill)
    {
        bill.Cancel();
        bills.Remove(bill);
    }

    private void ResetBillData()
    {
        billName = $"Custom Bill #{billID}";
        for (var i = 0; i < Ratios.Count(); i++)
        {
            textBuffers[i] = "0";
            RequestedAmount[Ratios[i]] = 0;
            TotalCost = new DefValueStack<NetworkValueDef, float>();
            ByProducts = new DefValueStack<NetworkValueDef, float>();
        }
    }

    //Drawing
    public void TryDrawBillDetails(Rect detailRect)
    {
        if (curDetailRequester == null) return;
        Find.WindowStack.ImmediateWindow(GetHashCode(), detailRect, WindowLayer.Dialog, () =>
        {
            detailRect = detailRect.AtZero();
            TWidgets.DrawColoredBox(detailRect, TColor.BGDarker, TColor.WindowBGBorderColor, 1);
            CustomNetworkBillUtility.DrawDetails(detailRect.ContractedBy(5), curDetailRequester);
        }, false, false, 0);
    }

    public void RequestDetails(CustomNetworkBill customNetworkBill)
    {
        if (curDetailRequester == customNetworkBill)
        {
            curDetailRequester = null;
            return;
        }
        curDetailRequester = customNetworkBill;
    }
}