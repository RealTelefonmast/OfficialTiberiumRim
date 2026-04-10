using System;
using UnityEngine;
using Verse;

namespace TeleCore.UI.DynaUI;

public static class UILayerRenderer
{
    //Todo: Layered rendering of additional popups within DynaUI
    public static void BeginLayeredView()
    {
    }

    public static void DrawLayeredViews()
    {
    }

    //
    public static void DrawImmediateLayer(int layer, Rect rect, Action renderAction)
    {
        Verse.Widgets.BeginGroup(rect);
        {
            Verse.Widgets.DrawMenuSection(rect);
            var leftRect = rect.LeftPartPixels(300).ContractedBy(5).Rounded();
            var rightRect = rect.RightPartPixels(200).Rounded();
        }
        Verse.Widgets.EndGroup();
    }
}