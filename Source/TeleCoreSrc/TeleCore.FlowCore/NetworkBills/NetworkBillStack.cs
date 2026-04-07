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
    public class NetworkBillStack : IExposable
    {
        //
        public static readonly float MarketPriceFactor = 2.4f;
        public static readonly float WorkAmountFactor = 10;
        
        //Details
        private CustomNetworkBill detailsRequester;

        //
        private Comp_NetworkBillsCrafter billStackOwner;
        private List<CustomNetworkBill> bills = new();

        //Temp Custom Bill
        public int billID = 1;
        public string billName = "";
        public Dictionary<CustomRecipeRatioDef, int> RequestedAmount = new();
        public string[] textBuffers;

        public DefValue<NetworkValueDef,float>[] TotalCost { get; set; }
        public int TotalWorkAmount => TotalCost.NullOrEmpty() ? 0 : TotalCost.Sum(m => (int)(m.Value * WorkAmountFactor));

        //
        public Building ParentBuilding => billStackOwner.parent;
        public Comp_NetworkBillsCrafter ParentComp => billStackOwner;
        public IEnumerable<NetworkComponent> ParentNetComps => UsedNetworks?.Select(n => ParentComp[n]) ?? null;
        public IEnumerable<NetworkDef> UsedNetworks => CurrentBill?.networkCost.Select(t => t.Def.networkDef)?.Distinct() ?? null;

        public List<CustomRecipeRatioDef> Ratios => ParentComp.Props.UsedRatioDefs;

        public List<CustomNetworkBill> Bills => bills;
        public CustomNetworkBill CurrentBill => bills.FirstOrDefault(c => c?.ShouldDoNow() ?? false);
        public int Count => bills.Count;

        public CustomNetworkBill this[int index] => bills[index];

        public NetworkBillStack(Comp_NetworkBillsCrafter parent)
        {
            billStackOwner = parent;
            textBuffers = new string[Ratios.Count];
            foreach (var recipe in Ratios)
            {
                RequestedAmount.Add(recipe, 0);
            }
            ResetBillData();
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref billID, "billID");
            Scribe_Values.Look(ref billName, "billName");
            Scribe_Collections.Look(ref RequestedAmount, "requestAmount");
            Scribe_Collections.Look(ref bills, "bills", LookMode.Deep, this);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                for (int i = 0; i < RequestedAmount.Count; i++)
                {
                    textBuffers[i] = RequestedAmount.ElementAt(i).Value.ToString();
                }
            }
        }

        public void CreateBillFromDef(CustomRecipePresetDef presetDefDef)
        {
            var totalCost = presetDefDef.desiredResources.Sum(t => (int)(t.Value * WorkAmountFactor));
            CustomNetworkBill customBill = new CustomNetworkBill(totalCost);
            customBill.billName = presetDefDef.defName;
            customBill.networkCost = NetworkBillUtility.ConstructCustomCost(presetDefDef.desiredResources);
            customBill.billStack = this;
            customBill.results = presetDefDef.Results;
            bills.Add(customBill);
        }

        public void TryCreateNewBill()
        {
            if (TotalCost == null || TotalCost.Sum(t => t.Value) <= 0) return;

            CustomNetworkBill customBill = new CustomNetworkBill(TotalWorkAmount);
            customBill.billName = billName;
            customBill.networkCost = new DefValue<NetworkValueDef,float>[TotalCost.Length];
            TotalCost.CopyTo(customBill.networkCost);
            customBill.billStack = this;
            customBill.results = RequestedAmount.Where(m => m.Value > 0).Select(m => new ThingDefCount(m.Key.result, m.Value)).ToList();
            bills.Add(customBill);
            billID++;

            //Clear Data
            ResetBillData();
        }

        public void PasteFromClipBoard(CustomNetworkBill clipBoardVal)
        {
            clipBoardVal.billStack = this;
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
            for (int i = 0; i < Ratios.Count(); i++)
            {
                textBuffers[i] = "0";
                RequestedAmount[Ratios[i]] = 0;
                TotalCost = null;
            }
        }

        //Drawing
        public void TryDrawBillDetails(Rect detailRect)
        {
            if (detailsRequester == null) return;
            Find.WindowStack.ImmediateWindow(this.GetHashCode(), detailRect, WindowLayer.Dialog, () =>
            {
                detailRect = detailRect.AtZero();
                TWidgets.DrawColoredBox(detailRect, TColor.BGDarker, TColor.WindowBGBorderColor, 1);
                CustomNetworkBillUtility.DrawDetails(detailRect.ContractedBy(5), detailsRequester);
            }, false, false, 0);
        }

        public void RequestDetails(CustomNetworkBill customNetworkBill)
        {
            if (detailsRequester == customNetworkBill)
            {
                detailsRequester = null;
                return;
            }
            detailsRequester = customNetworkBill;
        }
    }
}
