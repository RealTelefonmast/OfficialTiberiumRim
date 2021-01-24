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
        private static float maxLabelWidth = 0;

        public static readonly float MarketPriceTiberiumFactor = 1.9f;

        public ITab_CustomRefineryBills()
        {
            this.size = new Vector2(800f, 400f);
            this.labelKey = "TR_TibResourceRefiner";
            var metals = DefDatabase<ThingDef>.AllDefs.Where(t => t.IsMetal);
            textBuffers = new string[metals.Count()];
            foreach (var resource in metals)
            {
                MetalAmount.Add(resource, 0);
            }
        }

        public override void OnOpen()
        {
            base.OnOpen();
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
            Rect mainRect = new Rect(default, size).ContractedBy(15f);
            GUI.BeginGroup(mainRect);
            //Left Part

            float curY = 0;
            for (int i = 0; i < Metals.Count(); i++)
            {
                ResourceRow(new Rect(0, curY, mainRect.LeftHalf().width, 40f), Metals.ElementAt(i), i);
                curY += 42f;
            }

            //Right Part

            Widgets.Label(new Rect(0,0, mainRect.RightHalf().width, mainRect.RightHalf().height), "Current Cost: " + TotalCost);
            GUI.EndGroup();
        }

        private void ResourceRow(Rect rect, ThingDef resource, int index)
        {
            //Icon
            Rect iconRect = new Rect(rect.xMin, rect.y, 30, 30);
            Widgets.ThingIcon(iconRect, resource);
            //Label
            Vector2 vector = Text.CalcSize(resource.LabelCap);
            if (vector.x > maxLabelWidth)
                maxLabelWidth = vector.x;
            Rect labelRect = new Rect(iconRect.xMax, rect.y, vector.x, 30);
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(labelRect, resource.LabelCap);
            Text.Anchor = default;
            //Buttons
            //Rect buttonArray = new Rect(labelRect.xMax, rect.y, 100, 40);
            float buttonArrayX = iconRect.xMax + maxLabelWidth + 5;
            Rect buttonMO = new Rect(buttonArrayX, rect.y,30,30);
            Rect buttonMT = new Rect(buttonMO.xMax, rect.y, 40, 30);
            Rect buttonPT = new Rect(buttonMT.xMax, rect.y, 40, 30);
            Rect buttonPO = new Rect(buttonPT.xMax, rect.y, 30, 30);
            if (Widgets.ButtonText(buttonMO, "-1"))
            {
                MetalAmount[resource] -= 1;
            }
            if (Widgets.ButtonText(buttonMT, "-10"))
            {
                MetalAmount[resource] -= 10;
            }
            if (Widgets.ButtonText(buttonPT, "+10"))
            {
                MetalAmount[resource] += 10;
            }
            if (Widgets.ButtonText(buttonPO, "+1"))
            {
                MetalAmount[resource] += 1;
            }
            //Value Field
            Rect fieldRect = new Rect(buttonPO.xMax, rect.y, 60, 30);
            var temp = MetalAmount[resource];
            Widgets.TextFieldNumeric<int>(fieldRect, ref temp, ref textBuffers[index]); //(int)Widgets.HorizontalSlider(sliderRect, MetalAmount[resource], 0, 100, false, default, default, default, 1);
            MetalAmount[resource] = temp;
            
            //

        }
    }
}
