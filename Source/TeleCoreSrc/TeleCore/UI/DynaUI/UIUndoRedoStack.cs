using System;
using System.Collections.Generic;

namespace TeleCore.UI.DynaUI;

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