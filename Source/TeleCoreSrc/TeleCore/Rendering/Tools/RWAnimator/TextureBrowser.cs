using System.Collections.Generic;
using System.Linq;
using RimWorld;
using TeleCore.Rendering.UI.DynaUI;
using TeleCore.Rendering.UI.DynaUI.Editing;
using TeleCore.Static;
using UnityEngine;
using Verse;

namespace TeleCore.Rendering.Tools.RWAnimator;

public class TextureBrowser : DataBrowser<WrappedTexture>
{
    private static List<KeyValuePair<string, Texture2D>> AllContent;

    public TextureBrowser(UIElementMode mode) : base(mode)
    {
        AllContent = new List<KeyValuePair<string, Texture2D>>();
    }

    protected override IEnumerable<ModContentPack> BaseMods => ModDirectoryData.GetAllModsWithTextures();

    protected override List<WrappedTexture> GenerateItemsFromSearch(QuickSearchFilter filter, bool filterChanged)
    {
        if (AllContent.NullOrEmpty() || filterChanged) ReloadAssets();
        return AllContent.Where(t => filter.Matches($"{t.Key} {t.Value.name}"))
            .Select(t => new WrappedTexture(t.Key, t.Value)).ToList();
    }

    private void ReloadAssets()
    {
        AllContent.Clear();
        AllContent.AddRange(AllowedMods.SelectMany(m => m.GetContentHolder<Texture2D>().contentList).ToList());
    }

    //
    protected override string LabelFor(WrappedTexture element)
    {
        return element.Texture?.name;
    }

    protected override Texture2D IconFor(WrappedTexture element)
    {
        return (Texture2D) element.Texture;
    }

    protected override string SearchTextFor(WrappedTexture element)
    {
        return $"{element.Path} {element.Texture?.name}";
    }
}