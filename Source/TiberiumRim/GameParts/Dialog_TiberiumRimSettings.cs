using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class Dialog_TiberiumRimSettings : Window
    {
        public Dialog_TiberiumRimSettings()
        {
            closeOnClickedOutside = true;
        }

        protected override float Margin => 0f;

        public override Vector2 InitialSize => new Vector2(400f, 500f);

        public override void DoWindowContents(Rect inRect)
        {
            Widgets.DrawTextureFitted(inRect, TiberiumContent.MainMenu, 1f);
            if (Widgets.CloseButtonFor(inRect))
            {
                this.Close(true);
            }
            GUI.BeginGroup(inRect);

            float curY = 94;
            float yOffset = 60f;
            float yExtra = 20f;

            Rect difficulty = new Rect(51f, curY, 300f, yOffset);
            curY += yOffset + yExtra;
            Rect gameplay = new Rect(51, curY, 300f, yOffset);
            curY += yOffset + yExtra;
            Rect graphics = new Rect(51f, curY, 300f, yOffset);

            if (Widgets.ButtonText(difficulty, "Difficulty"))
            {
                this.Close();
                //Find.WindowStack.Add(new Dialog_DifficultySettings());
            }
            if (Widgets.ButtonText(gameplay, "Gameplay"))
            {
                this.Close();
                //Find.WindowStack.Add(new Dialog_GameplaySettings());
            }
            if (Widgets.ButtonText(graphics, "Graphics"))
            {
                this.Close();
                //Find.WindowStack.Add(new Dialog_GraphicsSettings());
            }
            GUI.EndGroup();
        }
    }
}
