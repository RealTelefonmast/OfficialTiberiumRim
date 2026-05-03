using System;
using System.Collections.Generic;

namespace TeleCore.Unsorted;

public class UndoRedoAction
{
}

public static class UIUndoRedoStack
{
    internal static Dictionary<Type, List<UndoRedoAction>> ActionsByContext = new();

    public static void DoNewAction(Type inContext)
    {
    }
}