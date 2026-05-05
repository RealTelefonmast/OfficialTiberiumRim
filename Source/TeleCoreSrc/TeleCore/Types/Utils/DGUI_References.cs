using System.Collections.Generic;

namespace TeleCore.Unsorted;

public static class DGUI_References
{
    private static readonly Dictionary<Rendering.UI.DynaUI.UIElement, HashSet<Rendering.UI.DynaUI.UIElement>> References = new();
    private static readonly Dictionary<Rendering.UI.DynaUI.UIElement, Rendering.UI.DynaUI.UIElement> ReferencesReverse = new();

    public static void Reference(Rendering.UI.DynaUI.UIElement forElement, Rendering.UI.DynaUI.UIElement element)
    {
        if (References.TryGetValue(forElement, out var reference))
            reference.Add(element);
        else
            References[forElement] = new HashSet<Rendering.UI.DynaUI.UIElement> { element };

        if (ReferencesReverse.ContainsKey(element)) ReferencesReverse[element] = forElement;
    }

    public static Rendering.UI.DynaUI.UIElement ParentOf(Rendering.UI.DynaUI.UIElement uiElement)
    {
        return ReferencesReverse.GetValueOrDefault(uiElement);
    }

    public static IReadOnlyCollection<Rendering.UI.DynaUI.UIElement> ChildrenOf(Rendering.UI.DynaUI.UIElement uiElement)
    {
        return References.GetValueOrDefault(uiElement);
    }
}