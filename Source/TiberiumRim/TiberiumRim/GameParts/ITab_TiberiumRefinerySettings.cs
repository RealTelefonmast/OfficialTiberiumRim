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
        public ITab_TiberiumRefinerySettings()
        {
            this.size = new Vector2(800f, 400f);
            this.labelKey = "TabHealth";
            this.tutorTag = "Health";
        }

        public override void OnOpen()
        {
            base.OnOpen();
        }

        private IEnumerable<ThingDef> Metals => DefDatabase<ThingDef>.AllDefs.Where(t => t.IsMetal);

        protected override void FillTab()
        {
            Rect mainRect = new Rect(default, size);
            Rect leftPart = mainRect.LeftHalf();
            Rect rightPart = mainRect.RightHalf();

            foreach (var metal in Metals)
            {
                
            }
        }
    }
}
