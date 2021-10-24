using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class Designator_TRMenu : Designator
    {
        //TODO: Add customizable tab for favorites
        //Static Data
        private static Designator_BuildFixed mouseOverGizmo;
        private static TRThingDef inactiveDef;

        //Settings
        private FactionDesignationDef SelectedFaction = FactionDesignationDefOf.Common;
        private TRThingCategoryDef SelectedCategory => cachedSelection[SelectedFaction];
        private Dictionary<FactionDesignationDef, TRThingCategoryDef> cachedSelection = new Dictionary<FactionDesignationDef, TRThingCategoryDef>();
        private Dictionary<FactionDesignationDef,DesignationTexturePack> TexturePacks = new Dictionary<FactionDesignationDef, DesignationTexturePack>();

        private List<ThingDef> HighLightOptions = new List<ThingDef>();

        private Vector2 scroller = Vector2.zero;
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

        public void Select(TRThingDef def)
        {
            SelectedFaction = def.factionDesignation;
            cachedSelection[SelectedFaction] = def.TRCategory;
        }

        public void MarkForHighlight(ThingDef def)
        {
            HighLightOptions.Add(def);
        }

        public override void DrawPanelReadout(ref float curY, float width)
        {
            if (CurrentDesignator == null && inactiveDef != null)
            {
                inactiveDef.IsActive(out var reason);
                if (reason.NullOrEmpty()) return;

                Vector2 bannerSize = TRWidgets.FittedSizeFor(TiberiumContent.LockedBanner, width + 12f);
                Find.WindowStack.ImmediateWindow(9357445, new Rect(1, ArchitectCategoryTab.InfoRect.yMin + 1, bannerSize.x, bannerSize.y), WindowLayer.GameUI, delegate
                {
                    Widgets.DrawTextureFitted(new Rect(0, 0, bannerSize.x, bannerSize.y), TiberiumContent.LockedBanner, 1f);
                }, false, false, 0);

                Rect reasonRect = new Rect(new Vector2(0, curY), new Vector2(width, Text.CalcHeight(reason, width)));
                Widgets.Label(reasonRect, reason);
                return;
            }
            CurrentDesignator?.DrawPanelReadout(ref curY, width);
        }

        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            Rect windowRect = new Rect(200f, UI.screenHeight - (560f + searchBarSize.y), MenuSize.x, MenuSize.y + searchBarSize.y);
            Find.WindowStack.ImmediateWindow(231674, windowRect, WindowLayer.GameUI, delegate
            {
                Rect searchBar = new Rect(new Vector2(MenuSize.x - searchBarSize.x, 0f), searchBarSize);
                DoSearchBar(searchBar);
                //SetupBG
                Rect menuRect = new Rect(0f, searchBarSize.y, windowRect.width, 526f);
                Widgets.DrawTextureRotated(menuRect, TexturePacks[SelectedFaction].BackGround, 0f);
                //Reduce Content Rect
                menuRect = new Rect(sideMargin, menuRect.y + topBotMargin, menuRect.width - sideMargin, menuRect.height - (topBotMargin * 2));
                GUI.BeginGroup(menuRect);
                FactionSideBar(3);
                Rect extraDes = new Rect(2, menuRect.height - 75, iconSize, iconSize);
                DrawDesignator(extraDes, DesignatorUtility.FindAllowedDesignator<Designator_Deconstruct>());
                extraDes.y = extraDes.yMax + 5;
                DrawDesignator(extraDes, DesignatorUtility.FindAllowedDesignator<Designator_Cancel>());
                Rect DesignatorRect = new Rect(iconSize + sideMargin, 0f, menuRect.width - (iconSize + sideMargin), menuRect.height);
                GUI.BeginGroup(DesignatorRect);
                var subCats = SelectedFaction.subCategories;
                Vector2 curXY = Vector2.zero;
                foreach (var cat in subCats)
                {
                    Rect tabRect = new Rect(curXY, tabSize);
                    Rect clickRect = new Rect(tabRect.x + 5, tabRect.y, tabRect.width - (10), tabRect.height);
                    Texture2D tex = cat == SelectedCategory || Mouse.IsOver(clickRect) ? TexturePacks[SelectedFaction].TabSelected : TexturePacks[SelectedFaction].Tab;
                    Widgets.DrawTextureFitted(tabRect, tex, 1f);
                    if (TRThingDefList.HasUnDiscovered(SelectedFaction, cat))
                    {
                        TRWidgets.DrawTextureInCorner(tabRect, TiberiumContent.Undiscovered, 7, TextAnchor.UpperRight, new Vector2(-6, 3));
                        //DrawUndiscovered(tabRect, new Vector2(-6, 3));
                        //Widgets.DrawTextureFitted(tabRect, TiberiumContent.Tab_Undisc, 1f);
                    }

                    Text.Anchor = TextAnchor.MiddleCenter;
                    Text.Font = GameFont.Small;
                    string catLabel = cat.LabelCap;
                    if (Text.CalcSize(catLabel).y > tabRect.width)
                    { Text.Font = GameFont.Tiny; }
                    Widgets.Label(tabRect, catLabel);
                    Text.Font = GameFont.Tiny;
                    Text.Anchor = 0;

                    AdjustXY(ref curXY, tabSize.x - 10f, tabSize.y, tabSize.x * 3);
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
                        if(!DebugSettings.godMode && def.hidden) continue;
                        if (def.IsActive(out string reason))
                        {
                            Designator(def, main, size, ref curXY);
                        }
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
            if (def.hidden)
            {
                Text.Font = GameFont.Medium;
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(rect, "DEV");
                Text.Anchor = default;
                Text.Font = GameFont.Small;
            }

            if (HighLightOptions.Contains(def))
            {
                Widgets.DrawTextureFitted(rect, TiberiumContent.Des_Undisc, 1);
            }

            if (!def.ConstructionOptionDiscovered)
            {
                TRWidgets.DrawTextureInCorner(rect, TiberiumContent.Undiscovered, 7, TextAnchor.UpperRight, new Vector2(-5, 5));
                //DrawUndiscovered(rect, new Vector2(-5, 5));
                //Widgets.DrawTextureFitted(rect, TiberiumContent.Des_Undisc, 1f);
            }

            if (mouseOver)
            {
                if (!def.ConstructionOptionDiscovered)
                    def.ConstructionOptionDiscovered = true;
                mouseOverGizmo = StaticData.GetDesignatorFor(def);
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
                if (TRThingDefList.HasUnDiscovered(des))
                {
                    TRWidgets.DrawTextureInCorner(partRect, TiberiumContent.Undiscovered, 8, TextAnchor.UpperRight);
                    //DrawUndiscovered(partRect);
                }

                if (Widgets.ButtonInvisible(partRect))
                {
                    SearchText = "";
                    SelectedFaction = des;
                }
            }
        }

        private void DrawUndiscovered(Rect rect, Vector2 offset = default)
        {
            float size = 7f;
            Rect topRight = new Rect(new Vector2(rect.xMax-size, rect.y) + offset, new Vector2(size, size));
            Widgets.DrawTextureFitted(topRight, TiberiumContent.Undiscovered, 1);
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
                //TODO: Add whatever belongs here
                default:
                    break;
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
            return TRThingDefList.AllDefs.Where(d => d.IsActive(out string s) && d.label.ToLower().Contains(SearchText.ToLower())).ToList();
        }
    }
}
