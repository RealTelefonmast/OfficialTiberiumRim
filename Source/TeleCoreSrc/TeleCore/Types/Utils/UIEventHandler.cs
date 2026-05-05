using UnityEngine;
using Verse;

namespace TeleCore.Unsorted;

public interface IFocusable
{
    public bool CanBeFocused { get; }
    public Rect FocusRect { get; }
    public int RenderLayer { get; }
}

public static class UIEventHandler
{
    private static readonly Rect?[] layers = new Rect?[255];
    private static bool IsInitialized;

    private static int CurrentLayer;
    public static Vector2 MouseOnScreen { get; private set; }
    public static IFocusable FocusedElement { get; private set; }
    public static Rendering.UI.DynaUI.UIElement[] Layers { get; } = new Rendering.UI.DynaUI.UIElement[255];

    public static bool IsFocused(IFocusable element)
    {
        return element.Equals(FocusedElement);
    }

    public static void RegisterLayer(Rendering.UI.DynaUI.UIElement element)
    {
        element.RenderLayer = CurrentLayer;
        Layers[CurrentLayer] = element;
        CurrentLayer++;
    }

    public static int GetLayerOf(Rendering.UI.DynaUI.UIElement element)
    {
        return -1;
    }

    public static bool ElementIsCovered(IFocusable element)
    {
        for (var i = element.RenderLayer; i > -1; i--)
        {
            if (CurrentLayer == i) continue;
            if (layers[i].HasValue && Mouse.IsOver(layers[i].Value))
            {
                TLog.Warning(
                    $"Tried to focus covered element: {Layers[element.RenderLayer]} at [{element.RenderLayer}] covered by [{i}]");
                return true;
            }
        }

        return false;
    }

    public static void Begin()
    {
        if (IsInitialized)
            TLog.Warning(
                $"More calls to {nameof(UIEventHandler)}.{nameof(Begin)} than {nameof(UIEventHandler)}.{nameof(End)}, make sure to close the scope correctly.");
        CurrentLayer = 0;
        MouseOnScreen = Event.current.mousePosition;
        IsInitialized = true;
    }

    public static void End()
    {
        MouseOnScreen = Vector2.zero;
        IsInitialized = false;
    }

    public static void StartFocusForced(IFocusable element)
    {
        if (element.CanBeFocused && !ElementIsCovered(element)) FocusedElement = element;
    }

    public static void StartFocus(IFocusable element, Rect? markedRect = null)
    {
        if (element.CanBeFocused && Mouse.IsOver(element.FocusRect) && !ElementIsCovered(element))
        {
            if (markedRect.HasValue) layers[element.RenderLayer] = markedRect.Value;
            FocusedElement = element;
        }
    }

    public static void StopFocus(IFocusable element)
    {
        if (IsFocused(element))
        {
            FocusedElement = null;
            layers[element.RenderLayer] = null;
        }
    }
}