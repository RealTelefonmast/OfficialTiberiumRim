using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    [StaticConstructorOnStartup]
    public static class CanvasCursor
    {
        public static readonly Texture2D CustomCursor_Drag = ContentFinder<Texture2D>.Get("UI/Menu/Animator/CursorCustom_Drag", true);
        public static readonly Texture2D CustomCursor_Rotate = ContentFinder<Texture2D>.Get("UI/Menu/Animator/CursorCustom_Rotate", true);
        private static Vector2 cursorHotspot;

        private static ManipulationMode? lastMode;
        private static bool usingDefault, usingCustom;

        public static void Notify_TriggeredMode(ManipulationMode? maniMode)
        {
            return;
            if (lastMode == maniMode) return;
            var shouldReset = maniMode == null || maniMode == ManipulationMode.None;
            var newMode = !shouldReset && (maniMode != ManipulationMode.None || maniMode != lastMode);
            if (newMode)
            {
                switch (maniMode)
                {
                    case ManipulationMode.Move:
                        cursorHotspot = new Vector2(CustomCursor_Drag.width / 2f, CustomCursor_Drag.height / 2f);
                        Cursor.SetCursor(CustomCursor_Drag, cursorHotspot, CursorMode.Auto);
                        break;
                    case ManipulationMode.Resize:
                        break;
                    case ManipulationMode.Rotate:
                        cursorHotspot = new Vector2(CustomCursor_Rotate.width / 2f, CustomCursor_Rotate.height / 2f);
                        Cursor.SetCursor(CustomCursor_Rotate, cursorHotspot, CursorMode.Auto);
                        break;
                }
                lastMode ??= maniMode;
                usingCustom = true;
                usingDefault = false;
            }
            else if (usingCustom && !usingDefault && shouldReset)
            {
                CustomCursor.Deactivate();
                CustomCursor.Activate();
                lastMode = null;
                usingCustom = false;
                usingDefault = true;
            }
        }
    }

}
