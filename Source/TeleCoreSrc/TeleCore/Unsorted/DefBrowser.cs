using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace TeleCore.Unsorted;

public class DefBrowserSettings
{
    public Predicate<Def> filter;
}

public class DefBrowser : DataBrowser<Def>
{
    private readonly List<Def> _cachedData;
    private readonly DefBrowserSettings _settings;

    public DefBrowser(UIElementMode mode, DefBrowserSettings settings = null) : base(mode)
    {
        _settings = settings ?? new DefBrowserSettings();
        _cachedData = new List<Def>();
    }

    public DefBrowser(Vector2 pos, Vector2 size, UIElementMode mode, DefBrowserSettings settings = null) : base(pos,
        size, mode)
    {
        _settings = settings ?? new DefBrowserSettings();
        _cachedData = new List<Def>();
    }

    protected override IEnumerable<ModContentPack> BaseMods => ModDirectoryData.GetAllModsWithDefs();

    protected override List<Def> GenerateItemsFromSearch(QuickSearchFilter filter, bool filterChanged)
    {
        if (_cachedData.NullOrEmpty() || filterChanged)
        {
            _cachedData.Clear();
            foreach (var contentPack in AllowedMods)
                foreach (var def in contentPack.AllDefs)
                    if (_settings.filter(def) && filter.Matches(def.defName))
                        _cachedData.Add(def);
        }

        return _cachedData;
    }

    protected override string LabelFor(Def element)
    {
        return element.defName;
    }

    protected override Texture2D IconFor(Def element)
    {
        if (element is ThingDef thingDef)
            return (Texture2D)(thingDef.graphicData?.Graphic?.MatSingle?.mainTexture ?? BaseContent.BadTex);
        if (element is FleckDef fleckDef)
        {
            var texture = TWidgets.TextureForFleckMote(fleckDef);
            if (texture != null) return texture;
        }

        return base.IconFor(element);
    }

    protected override string SearchTextFor(Def element)
    {
        return element.defName;
    }
}