using TR.TextureContent;
using UnityEngine;
using Verse;

namespace TR;

public class Dialog_TiberiumRimSettings : Window
{
    public Dialog_TiberiumRimSettings()
    {
        closeOnClickedOutside = true;
    }

    protected override float Margin => 0f;

    public override Vector2 InitialSize => new(400f, 500f);

    public override void DoWindowContents(Rect inRect)
    {
        Widgets.DrawTextureFitted(inRect, TiberiumContent.MainMenu, 1f);
        if (Widgets.CloseButtonFor(inRect)) Close();
        GUI.BeginGroup(inRect);

        float curY = 94;
        var yOffset = 60f;
        var yExtra = 20f;

        var difficulty = new Rect(51f, curY, 300f, yOffset);
        curY += yOffset + yExtra;
        var gameplay = new Rect(51, curY, 300f, yOffset);
        curY += yOffset + yExtra;
        var graphics = new Rect(51f, curY, 300f, yOffset);

        if (Widgets.ButtonText(difficulty, "Difficulty")) Close();
        //Find.WindowStack.Add(new Dialog_DifficultySettings());
        if (Widgets.ButtonText(gameplay, "Gameplay")) Close();
        //Find.WindowStack.Add(new Dialog_GameplaySettings());
        if (Widgets.ButtonText(graphics, "Graphics")) Close();
        //Find.WindowStack.Add(new Dialog_GraphicsSettings());
        GUI.EndGroup();
    }
}