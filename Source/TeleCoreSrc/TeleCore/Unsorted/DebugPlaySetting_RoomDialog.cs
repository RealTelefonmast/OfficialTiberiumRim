using TeleCore.UI;
using UnityEngine;
using Verse;

namespace TeleCore.Unsorted;

public class DebugPlaySetting_RoomDialog : PlaySettingsWorker
{
    public override bool Visible => DebugSettings.godMode;
    public override Texture2D Icon => BaseContent.BadTex;
    public override string Description => "Toggle Debug Room Dialog";
    public override bool ShowOnMapView => true;

    public override void OnToggled(bool isActive)
    {
        if (isActive)
        {
            Find.WindowStack.Add(new Dialog_DebugRoomTrackers());
        }
        else
        {
            Find.WindowStack.TryRemove(typeof(Dialog_DebugRoomTrackers), true);
        }
    }
}