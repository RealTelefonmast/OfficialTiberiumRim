using UnityEngine;

namespace TeleCore.Types.Utils;

public static class DUI
{
    public static Rendering.UI.DynaUI.UIElement New(Rect rect)
    {
        var el = UIState.Register(rect);
        return el;
    }
}