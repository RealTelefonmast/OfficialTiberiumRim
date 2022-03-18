using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace TiberiumRim
{
    public class NetworkBillStack : IExposable
    {
        private Comp_NetworkStructureCrafter billStackOwner;
        private List<CustomNetworkBill> bills = new ();

        //Temp Custom Bill
        public int billID = 0;
        public string billName = "";
        public Dictionary<AtomicRecipeDef, int> RequestedAmount = new ();
        public string[] textBuffers;

        public DefValue<NetworkValueDef>[] TotalCost { get; set; }
        public int TotalWorkAmount => TotalCost.NullOrEmpty() ? 0 : TotalCost.Sum(m => (int)(m.Value * ITab_CustomRefineryBills.WorkAmountFactor));

        //
        public Building ParentBuilding => billStackOwner.parent;
        public Comp_NetworkStructureCrafter ParentComp => billStackOwner;
        public NetworkComponent ParentTibComp => ParentComp[TiberiumDefOf.TiberiumNetwork];

        public List<CustomNetworkBill> Bills => bills;
        public CustomNetworkBill CurrentBill => bills.FirstOrDefault();
        public int Count => bills.Count;

        public NetworkBillStack(Comp_NetworkStructureCrafter parent)
        {
            billStackOwner = parent;
            textBuffers = new string[ITab_CustomRefineryBills.Recipes.Count()];
            foreach (var recipe in ITab_CustomRefineryBills.Recipes)
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
                for(int i = 0; i < RequestedAmount.Count; i++)
                {
                    textBuffers[i] = RequestedAmount.ElementAt(i).Value.ToString();
                }
            }
        }

        public void CreateBillFromDef(AtomicRecipePreset presetDef)
        {
            var totalCost = presetDef.desiredResources.Sum(t => (int)(t.Value * ITab_CustomRefineryBills.WorkAmountFactor));
            CustomNetworkBill customBill = new CustomNetworkBill(totalCost);
            customBill.billName = presetDef.defName;
            customBill.networkCost = ITab_CustomRefineryBills.ConstructCustomCost(presetDef.desiredResources);
            customBill.billStack = this;
            customBill.results = presetDef.desiredResources.Select(m => new ThingDefCount(m.Def.result, m.Value)).ToList();
            bills.Add(customBill);
        }

        public void TryCreateNewBill()
        {
            if (TotalCost == null || TotalCost.Sum(t => t.Value) <= 0) return;

            CustomNetworkBill customBill = new CustomNetworkBill(TotalWorkAmount);
            customBill.billName = billName;
            customBill.networkCost = new DefValue<NetworkValueDef>[TotalCost.Length];
            TotalCost.CopyTo(customBill.networkCost);
            customBill.billStack = this;
            customBill.results = RequestedAmount.Where(m => m.Value > 0).Select(m => new ThingDefCount(m.Key.result, m.Value)).ToList();
            bills.Add(customBill);
            billID++;

            //Clear Data
            ResetBillData();
        }

        public void PasteFromClipBoard()
        {
            var bill = ClipBoardUtility.Clipboard;
            bill.billStack = this;
            bills.Add(bill);
        }

        public void Delete(CustomNetworkBill bill)
        {
            bills.Remove(bill);
        }

        private void ResetBillData()
        {
            billName = "Custom Bill #" + billID;
            for (int i = 0; i < ITab_CustomRefineryBills.Recipes.Count(); i++)
            {
                textBuffers[i] = "0";
                RequestedAmount[ITab_CustomRefineryBills.Recipes.ElementAt(i)] = 0;
            }
        }

        public CustomNetworkBill this[int index] => bills[index];
    }
}
