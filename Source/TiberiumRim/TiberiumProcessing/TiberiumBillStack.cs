using System.Collections.Generic;
using System.Linq;
using Verse;

namespace TiberiumRim
{
    public class TiberiumBillStack : IExposable
    {
        private Comp_NetworkStructureCrafter billStackOwner;
        private List<CustomTiberiumBill> bills = new ();

        //New Bill Data
        public int billID = 0;
        public string billName = "";
        public Dictionary<ThingDef, int> MetalAmount = new ();
        public string[] textBuffers;

        public int TotalCost => MetalAmount.Sum(m => (int)(m.Key.BaseMarketValue * m.Value * ITab_CustomRefineryBills.MarketPriceFactor));
        public int TotalWorkAmount => MetalAmount.Sum(m => (int)(m.Key.BaseMarketValue * m.Value * ITab_CustomRefineryBills.WorkAmountFactor));

        public TiberiumBillStack(Comp_NetworkStructureCrafter parent)
        {
            billStackOwner = parent;
            textBuffers ??= new string[ITab_CustomRefineryBills.Metals.Count()];
            foreach (var resource in ITab_CustomRefineryBills.Metals)
            {
               MetalAmount.Add(resource, 0);
            }
            ResetBillData();
        }

        public Building ParentBuilding => billStackOwner.parent;
        public Comp_NetworkStructureCrafter ParentComp => billStackOwner;

        public List<CustomTiberiumBill> Bills => bills;

        public int Count => bills.Count;

        public void ExposeData()
        {
            Scribe_Values.Look(ref billID, "billID");
            Scribe_Values.Look(ref billName, "billName");
            Scribe_Collections.Look(ref MetalAmount, "metals");
            Scribe_Collections.Look(ref bills, "bills", LookMode.Deep, this);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                for(int i = 0; i < MetalAmount.Count; i++)
                {
                    textBuffers[i] = MetalAmount.ElementAt(i).Value.ToString();
                }
            }
        }

        public CustomTiberiumBill CurrentBill => bills.FirstOrDefault();

        public void CreateNewBill()
        {
            if (TotalCost <= 0) return;

            CustomTiberiumBill customBill = new CustomTiberiumBill(TotalWorkAmount);
            customBill.billName = billName;
            customBill.tiberiumCost = TotalCost;
            customBill.billStack = this;
            customBill.results = MetalAmount.Where(m => m.Value > 0).Select(m => new ThingDefCount(m.Key, m.Value)).ToList();
            bills.Add(customBill);
            billID++;

            //Clear Data
            ResetBillData();
        }

        public void PasteFromClipBoard()
        {
            var bill = TiberiumBillUtility.Clipboard;
            bill.billStack = this;
            bills.Add(bill);
        }

        public void Delete(CustomTiberiumBill bill)
        {
            bills.Remove(bill);
        }

        private void ResetBillData()
        {
            billName = "Custom Bill #" + billID;
            for (int i = 0; i < ITab_CustomRefineryBills.Metals.Count(); i++)
            {
                textBuffers[i] = "0";
                MetalAmount[ITab_CustomRefineryBills.Metals.ElementAt(i)] = 0;
            }
        }

        public CustomTiberiumBill this[int index] => bills[index];
    }
}
