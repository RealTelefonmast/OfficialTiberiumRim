using System.Collections.Generic;

namespace TeleCore.Unsorted;

public class UISystem
{
    private readonly Dictionary<UIElement, List<UIElement>> _children = new();
    private HashSet<UIElement> _elements = new();
    private Dictionary<UIElement, UIElement> _parents = new();

    internal void RegisterRelation(UIElement parent, UIElement child)
    {
        if (_children.ContainsKey(parent))
            _children[parent].Add(child);
        else
            _children[parent] = new List<UIElement> { child };
    }
}