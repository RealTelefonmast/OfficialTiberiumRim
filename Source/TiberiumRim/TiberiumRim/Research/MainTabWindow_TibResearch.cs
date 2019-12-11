using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using UnityEngine;

namespace TiberiumRim
{

    public class ResearchNode
    {
        public TResearchDef def;
        private List<TResearchDef> next;


        public List<TResearchDef> Previous => def.requisites.tiberiumResearch;
        public List<TResearchDef> Next => next;

        public ResearchNode(TResearchDef research)
        {
            def = research;
            foreach(TResearchDef def in DefDatabase<TResearchDef>.AllDefs.Where(t => t.requisites != null && t.requisites.tiberiumResearch.Contains(research)))
            {
                next.Add(def);
            }
        }
    }

    public class ResearchNodeTree
    {
        public void Setup()
        {

        }
    }

    public class MainTabWindow_TibResearch : MainTabWindow
    {
        private static readonly float partMargin = 20f;


        public List<ResearchNode> ResearchRoots = new List<ResearchNode>();
        public List<ResearchNode> Nodes = new List<ResearchNode>();

        //Research Selection
        private static readonly Vector2 selSize = new Vector2(150, 50);

        public override void PreOpen()
        {
            base.PreOpen();
            foreach(TResearchDef def in DefDatabase<TResearchDef>.AllDefs)
            {
                ResearchNode node = new ResearchNode(def);
                Nodes.Add(node);
                if (node.Previous.NullOrEmpty())
                {
                    ResearchRoots.Add(node);
                }
            }
            Log.Message("Research Nodes: " + Nodes.Count);
            Log.Message("Research Roots: " + ResearchRoots.Count);
        }

        public override void PostOpen()
        {
            base.PostOpen();
            //esearchRoots.AddRange(DefDatabase<TResearchDef>.AllDefs.Where(t => t.requisites?.tiberiumResearch.NullOrEmpty() ?? false));
        }

        protected override float Margin => 15f;
       // public override Vector2 InitialSize =>new Vector2(UI.screenWidth, UI.screenHeight * 0.6f);

        public override Vector2 RequestedTabSize => new Vector2(UI.screenWidth, UI.screenHeight * 0.6f);

        public override void DoWindowContents(Rect inRect)
        {
            Rect LeftRect = new Rect(0, 0, 200, inRect.height);
            Rect RightRect = new Rect(LeftRect.xMax, 0, inRect.width - LeftRect.width, inRect.height);
            Widgets.DrawMenuSection(RightRect);
            DrawResearch(RightRect);

            /*
            Rect rect = inRect;

            Rect LeftPart = new Rect(rect.x,              rect.y, rect.width * 0.25f, inRect.height).ContractedBy(partMargin);
            Rect RightPart = new Rect(LeftPart.xMax + partMargin, rect.y, rect.width * 0.75f, inRect.height).ContractedBy(partMargin);

            Widgets.DrawMenuSection(LeftPart);
            Widgets.DrawMenuSection(RightPart);
            */


        }

        private void DrawResearch(Rect rect)
        {
            GUI.BeginGroup(rect.ContractedBy(10f));
            Vector2 lastPos = new Vector2(rect.x, rect.y);
            //Log.Message("Drawing - LastPos: " + lastPos);
            foreach (ResearchNode root in ResearchRoots)
            {
                DrawResearchOption(new Rect(lastPos, selSize), root, ref lastPos);
                lastPos.x = rect.x;
            }
            GUI.EndGroup();
        }

        private void DrawResearchOption(Rect rect, ResearchNode node, ref Vector2 nextPos)
        {
            if (node.Next.Any())
            {
                nextPos.x += selSize.x;
                nextPos.y += 0;
            }
            else
            {
                nextPos.x += 0;
                nextPos.y += selSize.y;
            }
            Widgets.DrawWindowBackground(rect);
            Widgets.Label(rect, def.label);
            foreach(TResearchDef unlock in node.Next)
            {
                DrawResearchOption(new Rect(nextPos, selSize), Nodes.Find(n => n.def == unlock), ref nextPos);
            }
        }

        /*
        private float HeightFrom(TResearchDef def)
        {
            float val = selSize.y;
            int num = 0;
            for (num = 0; def.unlocks.Count > 0; num += def.unlocks.Count) { }
            if (num > 0)
                val += (num - 1) * selSize.y;
            foreach (var def2 in def.unlocks)
                val += HeightFrom(def2);
            return val;
        }

        private float WidthFrom(TResearchDef def)
        {
            float val = selSize.x;
            if (def.unlocks.Count > 0)
                val += selSize.x;
            foreach (var def2 in def.unlocks)
                val += WidthFrom(def2);
            return val;
        }
        */
    }
}
