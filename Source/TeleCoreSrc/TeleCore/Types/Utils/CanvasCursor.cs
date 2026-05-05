using TeleCore.Static;
using TeleCore.Unsorted;
using UnityEngine;
using Verse;

namespace TeleCore.Rendering.Tools.RWAnimator;

internal static class CanvasCursor
{
    //
    private static Vector2 cursorHotspot;
    private static ManipulationMode? lastMode;
    private static bool usingDefault, usingCustom;

    public static void Notify_TriggeredMode(ManipulationMode? maniMode)
    {
        if (lastMode == maniMode) return;
        var shouldReset = maniMode == null || maniMode == ManipulationMode.None;
        var newMode = !shouldReset && (maniMode != ManipulationMode.None || maniMode != lastMode);
        if (newMode)
        {
            switch (maniMode)
            {
                case ManipulationMode.Move:
                    cursorHotspot = new Vector2(TeleContentDB.CustomCursor_Drag.width / 2f,
                        TeleContentDB.CustomCursor_Drag.height / 2f);
                    Cursor.SetCursor(TeleContentDB.CustomCursor_Drag, cursorHotspot, CursorMode.Auto);
                    break;
                case ManipulationMode.Resize:
                    break;
                case ManipulationMode.Rotate:
                    cursorHotspot = new Vector2(TeleContentDB.CustomCursor_Rotate.width / 2f,
                        TeleContentDB.CustomCursor_Rotate.height / 2f);
                    Cursor.SetCursor(TeleContentDB.CustomCursor_Rotate, cursorHotspot, CursorMode.Auto);
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