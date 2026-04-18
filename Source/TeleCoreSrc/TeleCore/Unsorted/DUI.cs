using UnityEngine;

namespace TeleCore.Unsorted;

public static class DUI
{
    public static UIElement New(Rect rect)
    {
        var el = UIState.Register(rect);
        return el;
    }
}