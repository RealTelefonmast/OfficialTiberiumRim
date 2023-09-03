using System;
using UnityEngine;
using Verse;

namespace TR
{
    public class Dialog_Difficulty : Window
    {
        private Action SetEasy;
        private Action SetStandard;
        private Action SerHard;

        private Texture2D Banner => TiberiumContent.Banner;

        public Dialog_Difficulty(Action setEasy, Action setStandard, Action serHard)
        {
            this.SetEasy = setEasy;
            this.SetStandard = setStandard;
            this.SerHard = serHard;
            this.forcePause = true;
        }

        public override Vector2 InitialSize => new Vector2(750f, 450f);

        public override void DoWindowContents(Rect inRect)
        {
            Rect buttonRect = new Rect(0f, 0f, 250f, 75f);
            float scale = inRect.width / Banner.width;
            Rect bannerRect = new Rect(0, 0, Banner.width * scale, Banner.height * scale);
            Widgets.DrawShadowAround(bannerRect.ContractedBy(2f));
            Widgets.DrawTextureFitted(bannerRect, Banner, 1f);

            buttonRect.y += (Banner.height * scale) + 10f;

            float s = 15f;
            MakeButtonWithDesc(inRect, buttonRect, "Easy".Translate(), "EasySetting".Translate(), SetEasy, true, s, out buttonRect);
            MakeButtonWithDesc(inRect, buttonRect, "Standard".Translate(), "StandardSetting".Translate(), SetStandard, true, s, out buttonRect);
            MakeButtonWithDesc(inRect, buttonRect, "Hard".Translate(), "HardSetting".Translate(), SerHard, true, s, out buttonRect);

            string label = "SettingNotice".Translate();
            Widgets.Label(new Rect((inRect.width - label.GetWidthCached()) / 2, inRect.height - 25f, label.GetWidthCached(), 25f), label);
        }

        public void MakeButtonWithDesc(Rect inRect, Rect buttonRect, string label, string desc, Action action, bool isOn, float spacing, out Rect buttonRect2)
        {
            if (Widgets.ButtonText(buttonRect, label, true, false, isOn))
            {
                Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("ConfirmSettings".Translate(label), delegate
                {
                    action();
                    this.Close();
                }, false));
            }
            if (Mouse.IsOver(buttonRect))
            {
                Widgets.TextArea(new Rect(buttonRect.width + 5, buttonRect.y, inRect.width - buttonRect.width, inRect.height - buttonRect.y), desc, true);
            }
            buttonRect2 = new Rect(buttonRect.x, buttonRect.y + buttonRect.height + spacing, buttonRect.width, buttonRect.height);
        }
    }
}
