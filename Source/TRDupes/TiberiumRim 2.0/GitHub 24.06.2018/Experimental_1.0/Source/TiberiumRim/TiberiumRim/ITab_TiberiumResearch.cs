using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace TiberiumRim
{
    public class ITab_TiberiumResearch : ITab
    {
        public WorldComponent_TiberiumMissions Research;

        public Building_TiberiumResearchBench bench;

        public Vector2 scrollPosLeft = Vector2.zero;

        public ITab_TiberiumResearch()
        {
            this.labelKey = "TabResearch";
        }

        public Vector2 WinSize
        {
            get
            {
                return new Vector2(1280f, (float)UI.screenHeight - 350f);
            }
        }

        public override void OnOpen()
        {
            base.OnOpen();
            this.Research = Find.World.GetComponent<WorldComponent_TiberiumMissions>();
        }

        protected override void FillTab()
        {
            this.size = WinSize;
            this.bench = (base.SelThing as Building_TiberiumResearchBench);
            Rect rect = new Rect(0f, 0f, this.WinSize.x, this.WinSize.y);
            Rect rect2 = rect.ContractedBy(10f);
            rect2.height = 75f; 
            Widgets.DrawTextureFitted(rect2, TRMats.WorkBanner, 1f);
            Rect rect3 = new Rect(0f, 75, 300f, rect.height - 75f).ContractedBy(10f);
            Widgets.DrawMenuSection(rect3);
            GUI.BeginGroup(rect3);
            Widgets.BeginScrollView(new Rect(0f, 0f, rect3.width, rect3.height), ref this.scrollPosLeft, rect3, true);
            Widgets.EndScrollView();
            GUI.EndGroup();
            Rect rect4 = new Rect(300f, 75f, rect.width - 300f, rect.height - 75f).ContractedBy(10f);
            Widgets.DrawMenuSection(rect4);
        }
    }
}
