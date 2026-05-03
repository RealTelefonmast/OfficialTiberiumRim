using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace TeleCore.Unsorted;

public class FleckMoteBrowser : DataBrowser<Def>
{
    private List<Def> DataCached;

    public FleckMoteBrowser(Vector2 pos, Vector2 size, UIElementMode mode) : base(pos, size, mode)
    {
    }

    protected override IEnumerable<ModContentPack> BaseMods => ModDirectoryData.GetAllModsWithDefs();

    protected override List<Def> GenerateItemsFromSearch(QuickSearchFilter filter, bool filterChanged)
    {
        if (DataCached.NullOrEmpty() || filterChanged)
        {
            DataCached ??= new List<Def>();
            DataCached.Clear();
            foreach (var contentPack in AllowedMods)
            foreach (var def in contentPack.AllDefs)
                if (def is ThingDef { mote: not null } or FleckDef && filter.Matches(def.defName))
                    DataCached.Add(def);
        }

        return DataCached;
    }

    protected override string LabelFor(Def element)
    {
        return $"[{(element is FleckDef ? "Fleck" : "Mote")}]{element.defName}";
    }

    protected override Texture2D IconFor(Def element)
    {
        if (element is ThingDef moteDef)
            return (Texture2D)(moteDef.graphicData?.Graphic?.MatSingle?.mainTexture ?? BaseContent.BadTex);
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