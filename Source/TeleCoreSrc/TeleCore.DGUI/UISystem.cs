using System.Collections.Generic;

namespace TeleCore.DGUI;

public class UISystem
{
    private HashSet<UIElement> _elements = new();
    private Dictionary<UIElement, List<UIElement>> _children = new();
    private Dictionary<UIElement, UIElement> _parents = new();

    internal void RegisterRelation(UIElement parent, UIElement child)
    {
        if (_children.ContainsKey(parent))
        {
            _children[parent].Add(child);
        }
        else
        {
            _children[parent] = new List<UIElement>() { child };
        }
    }
}