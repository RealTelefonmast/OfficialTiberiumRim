using System;
using UnityEngine;
using Verse;

namespace TR.GameParts;

[StaticConstructorOnStartup]
public class Dialog_DifficultyPreset : Window
{
    private readonly Texture2D banner = ContentFinder<Texture2D>.Get("UI/Menu/Banner");
    private readonly Action easyAction;
    private readonly Action hardAction;
    private readonly Action standardAction;

    public Dialog_DifficultyPreset(Action easyAction, Action standardAction, Action hardAction)
    {
        this.easyAction = easyAction;
        this.standardAction = standardAction;
        this.hardAction = hardAction;
        forcePause = true;
    }

    public override Vector2 InitialSize => new(750f, 450f);

    public override void DoWindowContents(Rect inRect)
    {
        var buttonRect = new Rect(0f, 0f, 250f, 75f);
        var scale = inRect.width / banner.width;
        var bannerRect = new Rect(0, 0, banner.width * scale, banner.height * scale);
        Widgets.DrawShadowAround(bannerRect.ContractedBy(2f));
        Widgets.DrawTextureFitted(bannerRect, banner, 1f);
        buttonRect.y += banner.height * scale + 10f;

        var s = 15f;
        MakeButtonWithDesc(inRect, buttonRect, "TR_Easy".Translate(), "TR_EasySetting".Translate(), easyAction, true, s,
            out buttonRect);
        MakeButtonWithDesc(inRect, buttonRect, "TR_Standard".Translate(), "TR_StandardSetting".Translate(),
            standardAction, true, s, out buttonRect);
        MakeButtonWithDesc(inRect, buttonRect, "TR_Hard".Translate(), "TR_HardSetting".Translate(), hardAction, true, s,
            out buttonRect);

        string label = "SettingNotice".Translate();
        Widgets.Label(
            new Rect((inRect.width - label.GetWidthCached()) / 2, inRect.height - 25f, label.GetWidthCached(), 25f),
            label);
    }

    public void MakeButtonWithDesc(Rect inRect, Rect buttonRect, string label, string desc, Action action,
        bool isOn, float spacing, out Rect buttonRect2)
    {
        if (Widgets.ButtonText(buttonRect, label, true, false, isOn))
            Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("ConfirmSettings".Translate(label), delegate
            {
                action();
                Close();
            }));

        if (Mouse.IsOver(buttonRect))
            Widgets.TextArea(
                new Rect(buttonRect.width + 5, buttonRect.y, inRect.width - buttonRect.width,
                    inRect.height - buttonRect.y), desc, true);
        buttonRect2 = new Rect(buttonRect.x, buttonRect.y + buttonRect.height + spacing, buttonRect.width,
            buttonRect.height);
    }
}