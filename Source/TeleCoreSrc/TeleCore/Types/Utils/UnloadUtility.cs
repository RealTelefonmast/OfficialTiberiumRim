using System;

namespace TeleCore.Unsorted;

public static class UnloadUtility
{
    internal static Action MemoryUnloadEvent;
    internal static Action MemoryUnloadEventThreadSafe;

    public static void RegisterUnloadAction(Action action)
    {
        MemoryUnloadEvent += action;
    }

    public static void RegisterUnloadActionThreadSafe(Action action)
    {
        MemoryUnloadEventThreadSafe += action;
    }
}