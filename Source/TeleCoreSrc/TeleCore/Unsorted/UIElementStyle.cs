using UnityEngine;

namespace TeleCore.Unsorted;

public struct UIElementStyle
{
    public UIElementStyle()
    {
    }

    public Color BgColor { get; set; } = TColor.MenuSectionBGFillColor;
    public Color BorderColor { get; set; } = TColor.MenuSectionBGBorderColor;
    public bool HasTopBar { get; set; } = false;
    public string Label { get; set; }
    public string Title { get; set; }
}