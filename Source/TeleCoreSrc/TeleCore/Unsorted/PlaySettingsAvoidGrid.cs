using UnityEngine;
using Verse;

namespace TeleCore.Unsorted;

public class PlaySettingsAvoidGrid : PlaySettingsWorker
{
    public static bool DrawAvoidGridsAroundMouse;

    public override bool Visible => DebugSettings.godMode;
    public override Texture2D Icon => BaseContent.BadTex;
    public override string Description => "Toggle AvoidGrid View";
    public override bool ShowOnMapView => true;

    public override void OnToggled(bool isActive)
    {
        DrawAvoidGridsAroundMouse = isActive;
    }
}