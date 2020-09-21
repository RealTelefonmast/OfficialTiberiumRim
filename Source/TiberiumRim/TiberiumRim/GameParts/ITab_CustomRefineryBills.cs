using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class ITab_CustomRefineryBills : ITab
    {
        private Dictionary<ThingDef, int> MetalAmount = new Dictionary<ThingDef, int>();
        private string[] textBuffers;
        public static readonly float MarketPriceTiberiumFactor = 1.9f;

        public ITab_CustomRefineryBills()
        {
            this.size = new Vector2(800f, 400f);
            this.labelKey = "TR_TibResourceRefiner";
        }

        public override void OnOpen()
        {
            base.OnOpen();
            var metals = DefDatabase<ThingDef>.AllDefs.Where(t => t.IsMetal);
            textBuffers = new string[metals.Count()];
            foreach (var resource in metals)
            {
                MetalAmount.Add(resource, 0);
            }
        }

        protected override void CloseTab()
        {
            base.CloseTab();
        }

        public int TotalCost => MetalAmount.Sum(m => (int)(m.Key.BaseMarketValue * m.Value * MarketPriceTiberiumFactor));

        public TiberiumCost MainCost
        {
            get
            {
                TiberiumCost cost = new TiberiumCost();
                foreach (var i in MetalAmount)
                {
                    cost.costs.Add(new TiberiumTypeCost());
                }
                return cost;
            }
        }

        private IEnumerable<ThingDef> Metals => DefDatabase<ThingDef>.AllDefs.Where(t => t.IsMetal);

        protected override void FillTab()
        {
            Rect mainRect = new Rect(default, size).ContractedBy(5f);
            Rect leftPart = mainRect.LeftHalf();
            Rect rightPart = mainRect.RightHalf();

            float curY = 0;
            for (int i = 0; i < Metals.Count(); i++)
            {
                ResourceRow(new Rect(0, curY, leftPart.width, 40f), Metals.ElementAt(i), i);
                curY += 42f;
            }
            Widgets.Label(rightPart, "Current Cost: " + TotalCost);
        }

        private void ResourceRow(Rect rect, ThingDef resource, int index)
        {
            Rect iconRect = rect.LeftPartPixels(40);

            Widgets.ThingIcon(iconRect, resource);
            Rect fieldRect = new Rect(iconRect.xMax, rect.y, 100, 30);
            var temp = MetalAmount[resource];
            Widgets.TextFieldNumeric<int>(fieldRect, ref temp, ref textBuffers[index]); //(int)Widgets.HorizontalSlider(sliderRect, MetalAmount[resource], 0, 100, false, default, default, default, 1);
            MetalAmount[resource] = temp;
        }
    }
}
