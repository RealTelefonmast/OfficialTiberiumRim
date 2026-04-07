using System.Collections.Generic;

namespace TeleCore.SDGUI;

public static class DGUI_References
{
    private static readonly Dictionary<UIElement, HashSet<UIElement>> References = new Dictionary<UIElement, HashSet<UIElement>>();
    private static readonly Dictionary<UIElement, UIElement> ReferencesReverse = new Dictionary<UIElement, UIElement>();

    public static void Reference(UIElement forElement, UIElement element)
    {
        if (References.TryGetValue(forElement, out var reference))
        {
            reference.Add(element);
        }
        else
        {
            References[forElement] = new HashSet<UIElement> { element };
        }

        if (ReferencesReverse.ContainsKey(element))
        {
            ReferencesReverse[element] = forElement;
        }
    }

    public static UIElement ParentOf(UIElement uiElement)
    {
        return ReferencesReverse.GetValueOrDefault(uiElement);
    }

    public static IReadOnlyCollection<UIElement> ChildrenOf(UIElement uiElement)
    {
        return References.GetValueOrDefault(uiElement);
    }
}