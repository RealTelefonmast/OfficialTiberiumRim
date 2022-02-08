using System.Collections.Generic;
using System.Linq;
using Verse;

namespace TiberiumRim
{
    public class NetworkBillStack : IExposable
    {
        private Comp_NetworkStructureCrafter billStackOwner;
        private List<CustomTiberiumBill> bills = new ();

        //New Bill Data
        public int billID = 0;
        public string billName = "";
        public Dictionary<AtomicRecipeDef, int> RequestedAmount = new ();
        public string[] textBuffers;

        public int TotalCost => 0;//MetalAmount.Sum(m => (int)(m.Key.BaseMarketValue * m.Value * ITab_CustomRefineryBills.MarketPriceFactor));
        public int TotalWorkAmount => 0;//MetalAmount.Sum(m => (int)(m.Key.BaseMarketValue * m.Value * ITab_CustomRefineryBills.WorkAmountFactor));

        public NetworkBillStack(Comp_NetworkStructureCrafter parent)
        {
            billStackOwner = parent;
            textBuffers ??= new string[ITab_CustomRefineryBills.Recipes.Count()];
            foreach (var recipe in ITab_CustomRefineryBills.Recipes)
            {
                RequestedAmount.Add(recipe, 0);
            }
            ResetBillData();
        }

        public Building ParentBuilding => billStackOwner.parent;
        public Comp_NetworkStructureCrafter ParentComp => billStackOwner;
        public NetworkComponent ParentTibComp => ParentComp[TiberiumDefOf.TiberiumNetwork];

        public List<CustomTiberiumBill> Bills => bills;

        public int Count => bills.Count;

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

        public CustomTiberiumBill CurrentBill => bills.FirstOrDefault();

        public void CreateNewBill()
        {
            if (TotalCost <= 0) return;

            CustomTiberiumBill customBill = new CustomTiberiumBill(TotalWorkAmount);
            customBill.billName = billName;
            customBill.tiberiumCost = TotalCost;
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

        public void Delete(CustomTiberiumBill bill)
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

        public CustomTiberiumBill this[int index] => bills[index];
    }
}
