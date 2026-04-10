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
    public class ITab_TiberiumRefinerySettings : ITab
    {
        public Dictionary<ThingDef, int> MetalAmount = new Dictionary<ThingDef, int>();

        private readonly float marketPriceTiberiumFactor = 1.9f;

        public ITab_TiberiumRefinerySettings()
        {
            this.size = new Vector2(800f, 400f);
            this.labelKey = "TR_TibResourceRefiner";
        }

        public override void OnOpen()
        {
            base.OnOpen();
            foreach (var resource in DefDatabase<ThingDef>.AllDefs.Where(t => t.IsMetal))
            {
                MetalAmount.Add(resource, 0);
            }
        }

        public int TotalCost => MetalAmount.Sum(m => (int)(m.Key.BaseMarketValue * m.Value * marketPriceTiberiumFactor));

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
            foreach (var metal in Metals)
            {
                ResourceRow(new Rect(0, curY, leftPart.width, 40f), metal);
                curY += 42f;
            }

            Widgets.Label(rightPart, "Current Cost: " + TotalCost);
        }

        private void ResourceRow(Rect rect, ThingDef resource)
        {
            Rect iconRect = rect.LeftPartPixels(40);
            Rect sliderRect = new Rect(iconRect.xMax, rect.y, rect.width - 40, 40);
            Widgets.ThingIcon(iconRect, resource);
            MetalAmount[resource] = (int)Widgets.HorizontalSlider(sliderRect, MetalAmount[resource], 0, 100, false, default, default, default, 1);
        }
    }
}
