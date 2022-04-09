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
    }

    public static class UIEventHandler
    {
        public static IFocusable FocusedElement { get; private set; }

        public static bool IsFocused(IFocusable element) => element.Equals(FocusedElement);

        public static void StartFocus(IFocusable element)
        {
            if (element.CanBeFocused && Mouse.IsOver(element.FocusRect))
            {
                FocusedElement = element;
            }
        }

        public static void StopFocus(IFocusable element)
        {
            if (IsFocused(element))
                FocusedElement = null;
        }
    }
}
