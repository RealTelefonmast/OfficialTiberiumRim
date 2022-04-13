using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public interface IFocusable
    {
        public bool CanBeFocused { get; }
        public Rect FocusRect { get; }
        public int RenderLayer { get; }
    }

    public static class UIEventHandler
    {
        private static Rect?[] layers = new Rect?[255];
        private static UIElement[] elementLayers = new UIElement[255];

        public static int CurrentLayer;
        public static Vector2 MouseOnScreen { get; private set; }
        public static IFocusable FocusedElement { get; private set; }
        public static UIElement[] Layers => elementLayers;

        public static bool IsFocused(IFocusable element) => element.Equals(FocusedElement);

        public static void RegisterLayer(UIElement element)
        {
            //TLog.Debug($"Registering {element} at {CurrentLayer}");
            
            element.RenderLayer = CurrentLayer;
            elementLayers[CurrentLayer] = element;
            CurrentLayer++;
        }

        public static bool ElementIsCovered(IFocusable element)
        {
            for (int i = element.RenderLayer; i > -1; i--)
            {
                if (CurrentLayer == i) continue;
                if (layers[i].HasValue && Mouse.IsOver(layers[i].Value))
                {
                    TLog.Debug($"Tried to focus covered element: {elementLayers[element.RenderLayer]} at [{element.RenderLayer}] covered by [{i}]");
                    return true;
                }
            }
            return false;
        }


        public static void Notify_MouseOnScreen(Vector2 mousePos)
        {
            MouseOnScreen = mousePos;
        }

        public static void StartFocusForced(IFocusable element)
        {
            if (element.CanBeFocused && !ElementIsCovered(element))
            {
                FocusedElement = element;
            }
        }

        public static void StartFocus(IFocusable element, Rect? markedRect = null)
        {
            if (element.CanBeFocused && Mouse.IsOver(element.FocusRect) && !ElementIsCovered(element))
            {
                if (markedRect.HasValue)
                {
                    layers[element.RenderLayer] = markedRect.Value;
                }
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
}
