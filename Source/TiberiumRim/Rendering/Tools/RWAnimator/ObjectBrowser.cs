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
    public class ObjectBrowser : UIElement
    {
        private QuickSearchWidget searchWidget;
        //public override UIElementMode UIMode => UIElementMode.Static;

        //private List<ThingDef> searchList;
        private List<WrappedTexture> textureList;
        private Vector2 scrollPos = Vector2.zero;

        public override string Label => "Object Browser";

        private Rect MainRect => Rect.BottomPartPixels(Rect.height - TopRect.height);
        private Rect SearchWidgetRect => MainRect.TopPartPixels(QuickSearchWidget.WidgetHeight);
        private Rect SearchAreaRect => MainRect.BottomPartPixels(MainRect.height - QuickSearchWidget.WidgetHeight).ContractedBy(1);
        private Rect ScrollRect => SearchAreaRect.BottomPart(.95f);
        private Rect ScrollRectInner => new Rect(ScrollRect.x, ScrollRect.y, ScrollRect.width, textureList.Count * 30);
        private Rect InfoRect => SearchAreaRect.TopPart(0.05f);

        public ObjectBrowser(Rect rect, UIElementMode mode) : base(rect, mode)
        {
            searchWidget = new QuickSearchWidget();
        }

        private int startIndex, endIndex;
        private int indexRange;

        protected override void DrawContents(Rect inRect)
        {
            //
            searchWidget.OnGUI(SearchWidgetRect, CheckSearch);

            if (textureList == null) return;
            //Rect listRect = inRect.BottomPartPixels(inRect.height - QuickSearchWidget.WidgetHeight);
            //Rect innerRect = new Rect(ScrollRect.x, ScrollRect.y, ScrollRect.width, textureList.Count * 30);
            float curY = 0;

            Widgets.BeginScrollView(ScrollRect, ref scrollPos, ScrollRectInner, false);

            var optionSize = 40;

            startIndex = (int)(scrollPos.y / optionSize);
            indexRange = Math.Min((int)(ScrollRect.height / optionSize) + 1, textureList.Count);
            endIndex = startIndex + indexRange;
            if (startIndex >= 0 && endIndex <= textureList.Count)
            {
                for (int i = startIndex; i < endIndex; i++)
                {
                    curY = ScrollRect.y + i * optionSize;
                    Texture2D tex = (Texture2D)textureList[i].texture;
                    WidgetRow row = new WidgetRow(Rect.x, curY, gap: 4f);
                    row.Label($"[{i}]");
                    row.Icon(tex);
                    row.Label($"{tex.name}");

                    Rect pathLabelRect = new Rect(Rect.x, curY + WidgetRow.IconSize, Rect.width, optionSize);
                    GUI.color = TRColor.White075;
                    TRWidgets.DoTinyLabel(pathLabelRect, textureList[i].path);
                    GUI.color = Color.white;
                    
                    var optionRect = new Rect(Rect.x, curY, Rect.width, optionSize);
                    if (Mouse.IsOver(optionRect))
                    {
                        DragAndDropData = textureList[i];
                        Widgets.DrawHighlight(optionRect);
                    }
                }
            }

            Widgets.EndScrollView();

            Text.Anchor = TextAnchor.LowerRight;
            Widgets.Label(InfoRect, $"Showing {indexRange} of {textureList.Count} items...\n[{startIndex}...{endIndex}]");
            Text.Anchor = default;

        }

        private void CheckSearch()
        {
            //
            textureList = TiberiumRimMod.mod.Content.textures.contentList.Where(t => searchWidget.filter.Matches($"{t.Key} {t.Value.name}"))
                .Select(t => new WrappedTexture(t.Key, t.Value)).ToList();
            //searchList = DefDatabase<ThingDef>.AllDefs.Where(t => searchWidget.filter.Matches(t)).ToList();
        }
    }
}
