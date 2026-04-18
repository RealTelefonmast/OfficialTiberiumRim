using System;

namespace TeleCore.Unsorted;

public class ElementSelectedEventArgs : EventArgs
{
    public ElementSelectedEventArgs(UIElement element, int index)
    {
        Element = element;
        Index = index;
    }


    public UIElement Element { get; }

    public int Index { get; }
}