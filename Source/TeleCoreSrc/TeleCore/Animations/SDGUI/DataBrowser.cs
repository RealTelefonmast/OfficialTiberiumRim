using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace TeleCore;

public class DataBrowser<T> : UIElement
{
    private const float _OptionSize = 40;

    //
    private readonly Dictionary<string, ScribeDictionary<string, bool>> _modSourceSettings = new();

    //
    private List<T> dataList;
    private int indexRange;

    //
    private bool openFilter;
    private Vector2 scrollPos = Vector2.zero;

    private int startIndex, endIndex;

    public DataBrowser(UIElementMode mode) : base(mode)
    {
        Title = "Data Browser";
        bgColor = TColor.BGP3;

        //
        SearchWidget = new QuickSearchWidget();
        dataList = new List<T>();
    }

    public DataBrowser(Vector2 pos, Vector2 size, UIElementMode mode) : base(pos, size, mode)
    {
        Title = "Data Browser";
        bgColor = TColor.BGP3;

        //
        SearchWidget = new QuickSearchWidget();
        dataList = new List<T>();
    }

    public override string Label => "Data Browser";

    protected QuickSearchWidget SearchWidget { get; }

    protected virtual IEnumerable<ModContentPack> BaseMods => null;

    protected IEnumerable<ModContentPack> AllowedMods => BaseMods?.Where(m => AllowsModInDataBrowser(typeof(T), m));

    //
    private Rect MainRect => Rect.BottomPartPixels(Rect.height - TopRect.height);
    private Rect SearchWidgetRect => MainRect.TopPartPixels(QuickSearchWidget.WidgetHeight);

    private Rect SearchAreaRect =>
        MainRect.BottomPartPixels(MainRect.height - QuickSearchWidget.WidgetHeight).ContractedBy(1);

    private Rect ScrollRect => SearchAreaRect.BottomPartPixels(SearchAreaRect.height - _OptionSize);

    private Rect ScrollRectInner => new(ScrollRect.x, ScrollRect.y, ScrollRect.width, dataList.Count * _OptionSize);

    private Rect InfoRect => SearchAreaRect.TopPartPixels(_OptionSize / 2);

    protected override void DrawTopBarExtras(Rect topRect)
    {
        var filterButton = topRect.RightPartPixels(topRect.height).ContractedBy(2);
        if (Widgets.ButtonImage(filterButton, TeleContent.BurgerMenu))
        {
            openFilter = !openFilter;

            //Reload
            if (openFilter == false)
                GenerateItemsFromSearch(SearchWidget.filter, true);
        }
    }

    protected void DrawElementOption(Rect optionRect, T element, int index)
    {
        var row = new WidgetRow(Rect.x, optionRect.y, gap: 4f);
        row.Label($"[{index + 1}]");
        row.Icon(IconFor(element));
        row.Label(LabelFor(element));


        //TooltipHandler.TipRegion(new Rect(ScrollRectInner.x, curY, ScrollRectInner.width, _OptionSize), PathLabelMarked(textureList[i].Path, searchWidget.filter.searchText));

        var pathLabelResizec = SearchTextResized(SearchTextFor(element), optionRect.width);
        var pathLabelMarked = PathLabelMarked(pathLabelResizec, SearchWidget.filter.searchText);
        var pathLabelSize = Text.CalcSize(pathLabelMarked);
        var pathLabelX = pathLabelSize.x > optionRect.width
            ? optionRect.x - (pathLabelSize.x - optionRect.width)
            : optionRect.x;
        var pathLabelRect =
            new Rect(pathLabelX, optionRect.y + WidgetRow.IconSize, pathLabelSize.x, optionRect.height);
        GUI.color = TColor.White075;
        TWidgets.DoTinyLabel(pathLabelRect, pathLabelMarked);
        GUI.color = Color.white;
    }

    protected override void DrawContentsBeforeRelations(Rect inRect)
    {
        //
        SearchWidget.OnGUI(SearchWidgetRect, DoSearchOnGUI);

        GUI.color = TColor.MenuSectionBGBorderColor;
        Widgets.DrawLineHorizontal(SearchWidgetRect.x, SearchWidgetRect.yMax + 4, SearchWidgetRect.width);
        GUI.color = Color.white;

        if (openFilter)
        {
            var filterTop = SearchAreaRect.TopPartPixels(35).Rounded();
            var selectionRect = SearchAreaRect.BottomPartPixels(SearchAreaRect.height - filterTop.height).Rounded();
            Widgets.DrawBoxSolid(SearchAreaRect, TColor.WindowBGFillColor);

            Text.Anchor = TextAnchor.MiddleCenter;
            Text.Font = GameFont.Medium;
            GUI.color = TColor.White075;
            Widgets.Label(filterTop, "Select Mods To Browse From");
            GUI.color = Color.white;
            Text.Font = default;
            Text.Anchor = default;

            var listing = new Listing_Standard();
            listing.Begin(selectionRect);

            var i = 0;
            foreach (var mod in BaseMods)
            {
                var var = AllowsModInDataBrowser(typeof(T), mod);
                if (i % 2 == 0)
                    listing.DoBGForNext(TColor.White005);

                listing.CheckboxLabeled(mod.Name, ref var);

                //Update Settings
                SetDataBrowserSettings(typeof(T), mod.Name, var);
                i++;
            }

            listing.End();
            return;
        }

        if (dataList == null) return;
        //

        float curY = 0;
        Widgets.BeginScrollView(ScrollRect, ref scrollPos, ScrollRectInner, false);
        {
            startIndex = (int)(scrollPos.y / _OptionSize);
            indexRange = Math.Min((int)(ScrollRect.height / _OptionSize) + 1, dataList.Count);
            endIndex = startIndex + indexRange;
            if (startIndex >= 0 && endIndex <= dataList.Count)
                for (var i = startIndex; i < endIndex; i++)
                {
                    curY = ScrollRect.y + i * _OptionSize;
                    var optionRect = new Rect(Rect.x, curY, Rect.width, _OptionSize);
                    DrawElementOption(optionRect, dataList[i], i);

                    //Dragger
                    if (Mouse.IsOver(optionRect))
                    {
                        DragAndDropData = dataList[i];
                        Widgets.DrawHighlight(optionRect);
                    }
                }
        }
        Widgets.EndScrollView();

        Text.Font = GameFont.Tiny;
        Text.Anchor = TextAnchor.LowerRight;
        Widgets.Label(InfoRect, $"[Showing {indexRange} of {dataList.Count} Items]");
        Text.Anchor = default;
        Text.Font = GameFont.Small;
    }

    private void DoSearchOnGUI()
    {
        dataList = GenerateItemsFromSearch(SearchWidget.filter, true);
    }

    //
    private string SearchTextResized(string pathLabel, float width)
    {
        return pathLabel;
    }

    private string PathLabelMarked(string pathLabel, string searchText)
    {
        if (searchText.NullOrEmpty()) return pathLabel;

        var index = pathLabel.IndexOf(searchText, StringComparison.InvariantCultureIgnoreCase);
        if (index == -1) return pathLabel;

        var subPart = pathLabel.Substring(index, searchText.Length);
        return pathLabel.Replace(subPart, subPart.Colorize(Color.cyan));
    }

    //
    protected virtual List<T> GenerateItemsFromSearch(QuickSearchFilter filter, bool filterChanged)
    {
        return null;
    }

    protected virtual string SearchTextFor(T element)
    {
        return null;
    }

    protected virtual Texture2D IconFor(T element)
    {
        return BaseContent.BadTex;
    }

    protected virtual string LabelFor(T element)
    {
        return null;
    }

    #region ModSourceSettings

    protected bool AllowsModInDataBrowser(Type forType, ModContentPack mod)
    {
        if (!_modSourceSettings.TryGetValue(forType.ToString(), out var settings)) return true;
        return !settings.TryGetValue(mod.Name, out var value) || value;
    }

    protected void SetDataBrowserSettings(Type forType, string packName, bool value)
    {
        if (!_modSourceSettings.TryGetValue(forType.ToString(), out var settings))
        {
            settings = new ScribeDictionary<string, bool>(LookMode.Value, LookMode.Value);
            _modSourceSettings.Add(forType.ToString(), settings);
        }

        if (!settings.ContainsKey(packName))
        {
            settings.Add(packName, value);
            return;
        }

        settings[packName] = value;
    }

    #endregion
}