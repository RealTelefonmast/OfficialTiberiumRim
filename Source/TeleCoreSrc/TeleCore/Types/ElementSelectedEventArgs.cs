using System;

namespace TeleCore.Types;

public class ElementSelectedEventArgs : EventArgs
{
    public ElementSelectedEventArgs(Rendering.UI.DynaUI.UIElement element, int index)
    {
        Element = element;
        Index = index;
    }


    public Rendering.UI.DynaUI.UIElement Element { get; }

    public int Index { get; }
}