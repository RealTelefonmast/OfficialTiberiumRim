using System.Collections.Generic;
using System.Collections.Specialized;
using TeleCore.Rendering.UI.DynaUI;
using UnityEngine;
using Verse;

namespace TeleCore.Rendering.Tools.RWAnimator;

public class ToolBar : UIElement
{
    public ToolBar(UIElementMode mode) : base(mode)
    {
    }

    public ToolBar(Rect rect, UIElementMode mode) : base(rect, mode)
    {
    }

    public ToolBar(Vector2 pos, Vector2 size, UIElementMode mode) : base(pos, size, mode)
    {
    }

    protected override void NotifyCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        base.NotifyCollectionChanged(sender, e);
        if (e.NewItems.Count > 0 && e.NewItems[0] is UIElement element) element.ToggleOpen();
    }

    protected override void DrawContentsBeforeRelations(Rect inRect)
    {
        Verse.Widgets.BeginGroup(inRect);
        var list = new List<ListableOption>();
        foreach (var element in ChildElements)
            list.Add(new ListableOption(element.Label, () => { element.ToggleOpen(); }));
        OptionListingUtility.DrawOptionListing(new Rect(0, 0, inRect.width, inRect.height), list);
        Verse.Widgets.EndGroup();
    }
}