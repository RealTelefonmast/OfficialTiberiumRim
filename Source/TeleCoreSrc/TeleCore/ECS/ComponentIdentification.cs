using System;
using System.Runtime.InteropServices;

namespace TeleCore.ECS;

public struct ComponentType
{
    private static int _nextPrime = 2;
    public int Prime { get; }
    public Type Type { get; }

    public ComponentType(Type type)
    {
        Prime = GetNextPrime();
        Type = type;
    }

    private static int GetNextPrime()
    {
        int candidate = _nextPrime;
        while (true)
        {
            if (IsPrime(candidate))
            {
                _nextPrime = candidate + 1;
                return candidate;
            }
            candidate++;
        }
    }

    private static bool IsPrime(int n)
    {
        if (n <= 1) return false;
        if (n <= 3) return true;
        if (n % 2 == 0 || n % 3 == 0) return false;
        for (int i = 5; i * i <= n; i += 6)
        {
            if (n % i == 0 || n % (i + 2) == 0) return false;
        }
        return true;
    }
}

[StructLayout(LayoutKind.Sequential)]
public struct ArchetypeId
{
    public ulong Low;
    public ulong High;

    public ArchetypeId(ComponentType[] componentTypes)
    {
        Low = 1;
        High = 0;
        foreach (var type in componentTypes)
        {
            Multiply(type.Prime);
        }
    }

    private void Multiply(int prime)
    {
        ulong a = (ulong)prime;
        ulong lo = Low * a;
        ulong hi = High * a + ((Low * a) >> 32);
        Low = lo;
        High = hi;
    }

    public bool Equals(ArchetypeId other)
    {
        return Low == other.Low && High == other.High;
    }

    public override bool Equals(object obj)
    {
        return obj is ArchetypeId other && Equals(other);
    }

    public override int GetHashCode()
    {
        return (int)(Low ^ (Low >> 32) ^ High ^ (High >> 32));
    }
}

public unsafe struct ArchetypeStorage
{
    public ArchetypeId Id;
    public int* EntityIds;
    public void** ComponentArrays;
    public int EntityCount;
    public int Capacity;

    public ArchetypeStorage(ArchetypeId id, int initialCapacity)
    {
        Id = id;
        EntityCount = 0;
        Capacity = initialCapacity;
        EntityIds = (int*)Marshal.AllocHGlobal(sizeof(int) * initialCapacity);
        ComponentArrays = (void**)Marshal.AllocHGlobal(sizeof(void*) * World.RegisteredComponentTypes);
        for (int i = 0; i < World.RegisteredComponentTypes; i++)
        {
            ComponentArrays[i] = null;
        }
    }

    public void Dispose()
    {
        Marshal.FreeHGlobal((IntPtr)EntityIds);
        for (int i = 0; i < World.RegisteredComponentTypes; i++)
        {
            if (ComponentArrays[i] != null)
            {
                Marshal.FreeHGlobal((IntPtr)ComponentArrays[i]);
            }
        }
        Marshal.FreeHGlobal((IntPtr)ComponentArrays);
    }
}

public static class ComponentIdentification
{
    
}