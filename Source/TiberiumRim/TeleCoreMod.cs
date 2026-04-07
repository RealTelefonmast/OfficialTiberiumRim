using System;
using System.Reflection;
using TeleCore.Logging;
using TeleCore.Mod;
using UnityEngine;
using Verse;

namespace TR;

public class TeleCoreMod : TeleCore.Mod
{
    private static TeleCore.Harmony? _harmony;

    public TeleCoreMod(ModContentPack content) : base(content)
    {
        Mod = this;
        var curAss = Assembly.GetExecutingAssembly();
        TLog.Message($"{curAss.FullName}=>[TeleCore] - Init", Color.cyan);
        modSettings = GetSettings<TeleCoreSettings>();

        var discoveredAssemblies = AppDomain.CurrentDomain.GetAssemblies();
        foreach (var assembly in discoveredAssemblies)
        {
            if (assembly.GetCustomAttributes(typeof(TeleIdentifierAttribute), false).Length <= 0) continue;
            Log.Message($"Discovered TeleCore assembly: {assembly.FullName}");
            HarmonyInt.PatchAll(assembly);
        }

        TLog.Debug("Type check?");
        var type = typeof(DefInjectBase);
        TLog.Debug($"DefInjectBase: {type != null} :  {type?.Assembly?.FullName} : {type?.FullName}");
    }

    public static TeleCoreMod Mod { get; private set; }

    public static TeleCoreSettings Settings => (TeleCoreSettings)Mod.modSettings;

    public static TeleCore.Harmony HarmonyInt
    {
        get
        {
            TeleCore.Harmony.DEBUG = false;
            return _harmony ??= new TeleCore.Harmony("telefonmast.telecore");
        }
    }

    public override string SettingsCategory()
    {
        return "TeleCore";
    }

    public override void DoSettingsWindowContents(Rect inRect)
    {
        var ls = new Listing_Standard(GameFont.Small);
        ls.Begin(inRect);
        ls.Label("Some Settings");
        ls.CheckboxLabeled("Show Tele Tools in main menu", ref Settings.showToolsInMainMenu);
        ls.NewColumn();
        ls.End();
    }
}