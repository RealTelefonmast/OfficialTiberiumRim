using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using StoryFramework;
using Rect = UnityEngine.Rect;

namespace TiberiumRim
{
    public class Designator_TRMenu : Designator
    {
        private FactionDesignationDef SelectedFaction = FactionDesignationDefOf.Common;
        private TRThingCategoryDef SelectedCategory => cachedSelection[SelectedFaction];
        private Dictionary<FactionDesignationDef, TRThingCategoryDef> cachedSelection = new Dictionary<FactionDesignationDef, TRThingCategoryDef>();
        private Dictionary<FactionDesignationDef,DesignationTexturePack> TexturePacks = new Dictionary<FactionDesignationDef, DesignationTexturePack>();
        private Vector2 scroller = Vector2.zero;
        private static Designator_BuildFixed mouseOverGizmo;
        private static TRThingDef inactiveDef;
        private Rect currentRect;
        private string SearchText = "";

        private static Vector2 MenuSize = new Vector2(370, 526);
        private static Vector2 tabSize = new Vector2(118, 30);
        private static Vector2 searchBarSize = new Vector2(125, 25);
        private static float topBotMargin = 10f;
        private static float sideMargin = 3f;
        private static float iconSize = 30f;

        public Designator_TRMenu()
        {
            foreach(FactionDesignationDef def in TRThingDefList.FactionDesignations)
            {
                TexturePacks.Add(def, new DesignationTexturePack(def));
                cachedSelection.Add(def, def.subCategories[0]);
            }
        }

        public override string LabelCap => CurrentDesignator?.LabelCap;
        public override string Desc => CurrentDesignator?.Desc;

        public override void DrawPanelReadout(ref float curY, float width)
        {
            if (inactiveDef != null)
            {
                GUI.color = Color.red;
                Text.Font = GameFont.Medium;
                string locked = "Currently Locked";
                Vector2 lockedSize = Text.CalcSize(locked);
                Rect lockedRect = new Rect(new Vector2(0, curY - lockedSize.y), lockedSize);
                Widgets.Label(lockedRect, locked);
                Text.Font = GameFont.Small;
                GUI.color = Color.white;
                string reason = "";
                IsActive(inactiveDef, out reason);
                Vector2 reasonSize = Text.CalcSize(reason);
                Rect reasonRect = new Rect(new Vector2(0, curY), reasonSize);
                Widgets.Label(reasonRect, reason);
                return;
            }
            CurrentDesignator?.DrawPanelReadout(ref curY, width);
        }     

        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth)
        {
            Rect windowRect = currentRect = new Rect(200f, UI.screenHeight - (560f + searchBarSize.y), MenuSize.x, MenuSize.y + searchBarSize.y);
            Find.WindowStack.ImmediateWindow(231674, windowRect, WindowLayer.GameUI, delegate
            {
                Rect searchBar = new Rect(new Vector2(MenuSize.x - searchBarSize.x, 0f), searchBarSize);
                DoSearchBar(searchBar);
                //SetupBG
                Rect menuRect = new Rect(0f, searchBarSize.y, windowRect.width, 526f);
                Widgets.DrawTextureRotated(menuRect, TexturePacks[SelectedFaction].BackGround, 0f);
                //Reduce Content Rect
                menuRect = new Rect(sideMargin, menuRect.y + topBotMargin, menuRect.width - sideMargin , menuRect.height - (topBotMargin * 2));
                GUI.BeginGroup(menuRect);
                    FactionSideBar(3);
                    Rect extraDes = new Rect(2, menuRect.height - 75, iconSize, iconSize);
                    DrawDesignator(extraDes, new Designator_Deconstruct());
                    extraDes.y = extraDes.yMax + 5;
                    DrawDesignator(extraDes, new Designator_Cancel());
                    Rect DesignatorRect = new Rect(iconSize + sideMargin, 0f, menuRect.width - (iconSize + sideMargin), menuRect.height);
                    GUI.BeginGroup(DesignatorRect);
                        var cats = SelectedFaction.subCategories;
                        Vector2 curXY = Vector2.zero;
                        foreach (var cat in cats)
                        {
                            Rect tabRect = new Rect(curXY, tabSize);
                            Rect clickRect = new Rect(tabRect.x + 5, tabRect.y, tabRect.width - (10), tabRect.height);
                            Texture2D tex = cat == SelectedCategory || Mouse.IsOver(clickRect) ? TexturePacks[SelectedFaction].TabSelected : TexturePacks[SelectedFaction].Tab;
                            Widgets.DrawTextureFitted(tabRect, tex, 1f);

                            Text.Anchor = TextAnchor.MiddleCenter;
                            Text.Font = GameFont.Small;
                            string catLabel = ("TRCat_" + cat.defName).Translate();
                            if (Text.CalcSize(catLabel).y > tabRect.width)
                            { Text.Font = GameFont.Tiny; }
                            Widgets.Label(tabRect, catLabel);
                            Text.Font = GameFont.Tiny;
                            Text.Anchor = 0;

                            AdjustXY(ref curXY, tabSize.x-10f, tabSize.y, tabSize.x * 3);
                            if (Widgets.ButtonInvisible(clickRect))
                            {
                                SearchText = "";
                                SetSelectedCat(cat);
                            }
                        }
                        DrawFactionCat(new Rect(0f, curXY.y, DesignatorRect.width, DesignatorRect.height - curXY.y), SelectedFaction, SelectedCategory);
                    GUI.EndGroup();                
                GUI.EndGroup();
            }, false, false, 0f);
            return new GizmoResult(GizmoState.Mouseover);
        }

        private void DoSearchBar(Rect textArea)
        {
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleLeft;

            if (SearchText.NullOrEmpty())
            {
                GUI.color = new Color(1, 1, 1, 0.75f);
                Widgets.Label(textArea.ContractedBy(2), "Search..");
                GUI.color = Color.white;
            }
            SearchText = Widgets.TextArea(textArea, SearchText, false);
            Text.Anchor = 0;
        }

        private void DrawFactionCat(Rect main, FactionDesignationDef faction, TRThingCategoryDef category)
        {
            if (faction != null && category != null)
            {             
                GUI.BeginGroup(main);
                    Vector2 size = new Vector2(80, 80);
                    Vector2 curXY = new Vector2(5f, 5f);
                    List<TRThingDef> things = SearchText.NullOrEmpty() ? TRThingDefList.Categorized[faction][category] : ItemsBySearch(SearchText);
                    Rect viewRect = new Rect(0f, 0f, main.width, 10 + ((float)(Math.Round((decimal)(things.Count / 4), 0, MidpointRounding.AwayFromZero) + 1) * size.x));
                    Rect scrollerRect = new Rect(0f, 0f, main.width, main.height+5);
                    Widgets.BeginScrollView(scrollerRect, ref scroller, viewRect, false);
                    mouseOverGizmo = null;
                    inactiveDef = null;
                    foreach (var def in things)
                    {
                        if (IsActive(def, out string s))
                            Designator(def, main, size, ref curXY);
                        else
                            InactiveDesignator(def, main, size, ref curXY);
                    }
                    Widgets.EndScrollView();
                GUI.EndGroup();
            }
        }

        private void Designator(TRThingDef def, Rect main, Vector2 size, ref Vector2 XY)
        {
            Rect rect = new Rect(new Vector2(XY.x, XY.y), size);
            GUI.color = new Color(1, 1, 1, 0.80f);
            bool mouseOver = Mouse.IsOver(rect);
            Texture2D tex = mouseOver ? TexturePacks[SelectedFaction].DesignatorSelected : TexturePacks[SelectedFaction].Designator;
            Widgets.DrawTextureFitted(rect, tex, 1f);
            GUI.color = mouseOver ? new Color(1, 1, 1, 0.45f) : Color.white;
            Widgets.DrawTextureFitted(rect.ContractedBy(2), def.uiIcon, 1);
            GUI.color = Color.white;
            if (Mouse.IsOver(rect))
            {
                mouseOverGizmo = new Designator_BuildFixed(def);
                Text.Anchor = TextAnchor.UpperCenter;
                Widgets.Label(rect, def.LabelCap);
                Text.Anchor = 0;
                TooltipHandler.TipRegion(rect, def.LabelCap);
            }
            if (Widgets.ButtonInvisible(rect))
            { mouseOverGizmo.ProcessInput(null); }
            AdjustXY(ref XY, size.x, size.x, main.width, 5);
        }

        private void InactiveDesignator(TRThingDef def, Rect main, Vector2 size, ref Vector2 XY)
        {
            Rect rect = new Rect(new Vector2(XY.x, XY.y), size);
            GUI.color = Color.grey;
            bool mouseOver = Mouse.IsOver(rect);
            Texture2D tex = mouseOver ? TexturePacks[SelectedFaction].DesignatorSelected : TexturePacks[SelectedFaction].Designator;
            Widgets.DrawTextureFitted(rect, tex, 1f);
            Widgets.DrawTextureFitted(rect.ContractedBy(2), def.uiIcon, 1);
            GUI.color = Color.white;
            if (Mouse.IsOver(rect))
                inactiveDef = def;

            AdjustXY(ref XY, size.x, size.x, main.width, 5);
        }

        private Texture2D GrayscaleFrom(Texture2D tex)
        {
            var pix = tex.GetPixels32();
            for (int x = 0; x < tex.width; x++)
            {
                for (int y = 0; y < tex.height; y++)
                {
                    Color col = pix[x + y * tex.width];
                    var g = col.grayscale;
                    //tex.SetPixel(x,y,  new Color(g,g,g));
                }
            }
            return tex;
        }

        private void SetSelectedCat(TRThingCategoryDef def)
        {
            cachedSelection[SelectedFaction] = def;
        }

        private void AdjustXY(ref Vector2 XY, float xIncrement, float yIncrement, float maxWidth, float minX = 0f)
        {
            if(XY.x +(xIncrement*2) > maxWidth)
            {
                XY.y += yIncrement;
                XY.x = minX;
            }
            else
            {
                XY.x += xIncrement;
            }
        }

        private void DrawDesignator(Rect rect, Designator designator)
        {
            if (Widgets.ButtonImage(rect, designator.icon))
            {
                designator.ProcessInput(null);
            }
        }

        private void FactionSideBar(float yPos)
        {
            List<FactionDesignationDef> list = TRThingDefList.FactionDesignations.Where(CanSelect).ToList();
            for (int i = 0; i < list.Count; i++)
            {
                FactionDesignationDef des = list[i];
                Rect partRect = new Rect(0f, yPos + ((iconSize + 6) * i), iconSize, iconSize);
                bool sel = Mouse.IsOver(partRect) || SelectedFaction == des;
                GUI.color = sel ? Color.white : new Color(1f, 1f, 1f, 0.4f);
                Widgets.DrawTextureFitted(partRect, IconForFaction(des), 1f);
                GUI.color = Color.white;
                if (Widgets.ButtonInvisible(partRect))
                {
                    SearchText = "";
                    SelectedFaction = des;
                }
            }
        }

        public bool IsActive(TRThingDef def, out string reason)
        {
            reason = "";
            bool b = true;
            var sb = new StringBuilder();
            sb.AppendLine("Locked due to:\n");
            if (DebugSettings.godMode)
            {
                return true;
            }
            if (def.devObject)
            {
                return DebugSettings.godMode;
            }
            if (def.minTechLevelToBuild != TechLevel.Undefined && Faction.OfPlayer.def.techLevel < def.minTechLevelToBuild)
            { 
                b = false;
                sb.AppendLine("- Need min tech level: " + def.minTechLevelToBuild);
            }
            if (def.maxTechLevelToBuild != TechLevel.Undefined && Faction.OfPlayer.def.techLevel > def.maxTechLevelToBuild)
            {
                b = false;
                sb.AppendLine("- Need max tech level: " + def.maxTechLevelToBuild);
            }
            if (!def.IsResearchFinished)
            {
                b = false;
                string research = "";
                foreach (var res in def.researchPrerequisites)
                {
                    research += "   - " + res.LabelCap;
                }
                sb.AppendLine("- Need research:\n" + research);
            }
            if (def.HasStoryExtension())
            {
                bool r = false;
                b = b && StoryPatches.CanBeMade(def, ref r);
                if (!b)
                {
                    var story = def.GetModExtension<StoryThingDefExtension>();
                    string objectives = "";
                    foreach (var obj in story.objectiveRequisites)
                    {
                        objectives += "   - " + obj.LabelCap;
                    }
                    sb.AppendLine("- Need Objectives:\n" + objectives);
                }
            }
            if (def.buildingPrerequisites != null)
            {
                b = b && def.buildingPrerequisites.All(t => base.Map.listerBuildings.ColonistsHaveBuilding(t));
                if (!b)
                {
                    string buildings = "";
                    foreach (var build in def.buildingPrerequisites)
                    {
                        buildings += "   - " + build.LabelCap;
                    }
                    sb.AppendLine("- Need constructed buildings:\n" + buildings);
                }
            }
            reason = sb.ToString().TrimEndNewlines();
            return b;
        }

        public override AcceptanceReport CanDesignateCell(IntVec3 loc)
        {
            return false;
        }

        public override void ProcessInput(Event ev)
        {
        }

        private bool CanSelect(FactionDesignationDef faction)
        {
            if (faction == FactionDesignationDefOf.Tiberium)
                return DebugSettings.godMode;
            return IsUnlocked(faction);
        }

        private bool IsUnlocked(FactionDesignationDef faction)
        {
            return true;
        }

        private Designator_BuildFixed CurrentDesignator => mouseOverGizmo ?? Find.DesignatorManager.SelectedDesignator as Designator_BuildFixed;

        private Texture2D IconForIndex(int i)
        {
            switch (i)
            {
            }
            return BaseContent.BadTex;
        }

        private Texture2D IconForFaction(FactionDesignationDef faction)
        {
            if (faction == FactionDesignationDefOf.Common)
            {
                return TiberiumContent.CommonIcon;
            }
            if (faction == FactionDesignationDefOf.Forgotten)
            {
                return TiberiumContent.ForgottenIcon;
            }
            if (faction == FactionDesignationDefOf.GDI)
            {
                return TiberiumContent.GDIIcon;
            }
            if (faction == FactionDesignationDefOf.Nod)
            {
                return TiberiumContent.NodIcon;
            }
            if (faction == FactionDesignationDefOf.Scrin)
            {
                return TiberiumContent.ScrinIcon;
            }
            if (faction == FactionDesignationDefOf.Tiberium)
            {
                return TiberiumContent.TiberiumIcon;
            }
            return BaseContent.BadTex;
        }

        private List<TRThingDef> ItemsBySearch(string searchText)
        { 
            return TRThingDefList.AllDefs.Where(d => IsActive(d, out string s) && d.label.ToLower().Contains(SearchText.ToLower())).ToList();
        }
    }
}
