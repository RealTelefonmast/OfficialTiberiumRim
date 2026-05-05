using System.Collections.Generic;
using TeleCore.Types.Interfaces;

namespace TeleCore.Types;

public struct ComponentKind
{
    private int _id;
}

public class ComponentStore
{
    private Dictionary<ComponentKind, List<IComponent>> _componentByType = new();
}

public class ComponentStore<TComponent> where TComponent : IComponent
{
    private Dictionary<int, TComponent> _compById = new();
}