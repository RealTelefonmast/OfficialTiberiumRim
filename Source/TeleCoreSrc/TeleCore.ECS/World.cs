using System;
using System.Runtime.InteropServices;

namespace TeleCore.ECS;

public unsafe class World
{
    public static int RegisteredComponentTypes = 0;
    private static ComponentType[] _componentTypes = new ComponentType[1024]; // Adjust size as needed
    private ArchetypeStorage* _archetypeStorages;
    private int _archetypeCount;
    private int _archetypeCapacity;

    public World(int initialArchetypeCapacity = 1024)
    {
        _archetypeCapacity = initialArchetypeCapacity;
        _archetypeStorages = (ArchetypeStorage*)Marshal.AllocHGlobal(sizeof(ArchetypeStorage) * _archetypeCapacity);
        _archetypeCount = 0;
    }

    public static ComponentType RegisterComponentType<T>() where T : struct
    {
        var type = typeof(T);
        for (int i = 0; i < RegisteredComponentTypes; i++)
        {
            if (_componentTypes[i].Type == type)
            {
                return _componentTypes[i];
            }
        }
        var componentType = new ComponentType(type);
        _componentTypes[RegisteredComponentTypes++] = componentType;
        return componentType;
    }

    public ArchetypeStorage* GetOrCreateArchetype(ComponentType[] componentTypes)
    {
        var id = new ArchetypeId(componentTypes);
        for (int i = 0; i < _archetypeCount; i++)
        {
            if (_archetypeStorages[i].Id.Equals(id))
            {
                return &_archetypeStorages[i];
            }
        }

        if (_archetypeCount >= _archetypeCapacity)
        {
            // Resize logic here
        }

        _archetypeStorages[_archetypeCount] = new ArchetypeStorage(id, 1024); // Initial capacity
        return &_archetypeStorages[_archetypeCount++];
    }

    // Other methods...

    public void Dispose()
    {
        for (int i = 0; i < _archetypeCount; i++)
        {
            _archetypeStorages[i].Dispose();
        }
        Marshal.FreeHGlobal((IntPtr)_archetypeStorages);
    }
}