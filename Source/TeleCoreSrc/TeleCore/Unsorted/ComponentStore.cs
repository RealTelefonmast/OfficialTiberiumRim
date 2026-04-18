using System.Collections.Generic;

namespace TeleCore.Unsorted;

public struct ComponentKind
{
    private int _id;
}

public class ComponentStore
{
    private Dictionary<ComponentKind, List<IComponent>> _componentByType = new ();
}

public class ComponentStore<TComponent> where TComponent : IComponent
{
    private Dictionary<int, TComponent> _compById = new ();
}