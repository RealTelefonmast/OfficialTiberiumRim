using System.Collections.Generic;

namespace TeleCore.Types;

public class UISystem
{
    private readonly Dictionary<Rendering.UI.DynaUI.UIElement, List<Rendering.UI.DynaUI.UIElement>> _children = new();
    private HashSet<Rendering.UI.DynaUI.UIElement> _elements = new();
    private Dictionary<Rendering.UI.DynaUI.UIElement, Rendering.UI.DynaUI.UIElement> _parents = new();

    internal void RegisterRelation(Rendering.UI.DynaUI.UIElement parent, Rendering.UI.DynaUI.UIElement child)
    {
        if (_children.ContainsKey(parent))
            _children[parent].Add(child);
        else
            _children[parent] = new List<Rendering.UI.DynaUI.UIElement> { child };
    }
}