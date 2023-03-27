using TeleCore;
using UnityEngine;

namespace TiberiumRim.PlaySettings;

//row.ToggleableIcon(ref TRUtils.GameSettings().EVASystem, TiberiumContent.Icon_EVA, "Enable or disable the EVA", SoundDefOf.Mouseover_ButtonToggle);
//row.ToggleableIcon(ref TRUtils.GameSettings().RadiationOverlay, TiberiumContent.Icon_Radiation, "Toggle the Tiberium Radiation overlay.", SoundDefOf.Mouseover_ButtonToggle);

public class PlaySetting_EVA : PlaySettingsWorker
{
    public override bool Visible => true;
    public override Texture2D Icon => TiberiumContent.Icon_EVA;
    public override string Description => "TR.PlaySettings.EVA";
    
    public override void OnToggled(bool isActive)
    {
        TRUtils.GameSettings().EVASystem = isActive;
    }
}

public class PlaySetting_Radiation : PlaySettingsWorker
{
    public override bool Visible => true;
    public override Texture2D Icon => TiberiumContent.Icon_Radiation;
    public override string Description => "TR.PlaySettings.Radiation";
    
    public override void OnToggled(bool isActive)
    {
        TRUtils.GameSettings().RadiationOverlay = isActive;
    }
}