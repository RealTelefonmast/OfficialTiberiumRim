using System.Reflection;
using TeleCore.Logging;
using TeleCore.Utility;
using TeleCore.Utils;
using UnityEngine;
using Verse;

namespace TeleCore.Mod;

public class TeleCoreMod : Verse.Mod
{
    //Static Data
    private static HarmonyLib.Harmony teleCore;
    
    public TeleCoreMod(ModContentPack content) : base(content)
    {
        Mod = this;
        var assembly = Assembly.GetExecutingAssembly();
        var version = assembly.GetName().Version.ToString();
        
        //MethodInfo target = typeof(ToilFailConditions).GetNestedTypes(AccessTools.all).SelectMany(AccessTools.GetDeclaredMethods).First(mi => mi.Name.Contains("FailOnChildLearningConditions"));
        // TeleCore.Patch(
        //     original: AccessTools.Method(target.DeclaringType.MakeGenericType(new Type[] { typeof(IJobEndable) }), target.Name),
        //     transpiler: new HarmonyMethod(typeof(HarmonyPatcher), nameof(ReplaceDevelopmentalStageGeneralTranspiler))
        // );
        
        // var originalMethod = typeof(GenAttribute).GetMethod(nameof(GenAttribute.TryGetAttribute),new[] { typeof(MemberInfo), typeof(object) })
        //     ?.MakeGenericMethod(typeof(ReplacePatches.GenAttribute_Patch)); //replace YourType with the type you want to pass
        // TLog.Debug($"Found method: {originalMethod}");
        // var prefixMethodInfo = typeof(ReplacePatches.GenAttribute_Patch).GetMethod(nameof(ReplacePatches.GenAttribute_Patch.Prefix));
        // var prefixMethod = new HarmonyMethod(prefixMethodInfo);
        // TeleCore.Patch(originalMethod, prefix: prefixMethod);
        
        //
        TLog.Message($"[TeleCore] - Init", Color.cyan);
        modSettings = GetSettings<TeleCoreSettings>();

        //
        TeleCore.PatchAll(assembly);
        
        TeleParseHelper.Init();
    }

    public static TeleCoreMod Mod { get; private set; }

    public static HarmonyLib.Harmony TeleCore
    {
        get
        {
            HarmonyLib.Harmony.DEBUG = false;
            return teleCore ??= new HarmonyLib.Harmony("telefonmast.telecore");
        }
    }

    public static TeleCoreSettings Settings => (TeleCoreSettings) Mod.modSettings;

    public override string SettingsCategory()
    {
        return "TeleCore";
    }

    public override void WriteSettings()
    {
        base.WriteSettings();
    }

    public override void DoSettingsWindowContents(Rect inRect)
    {
        Listing_Standard ls = new Listing_Standard(GameFont.Small);
        ls.Begin(inRect);
        ls.Label("Some Settings");
        ls.CheckboxLabeled("Show Tele Tools in main menu", ref Settings.showToolsInMainMenu);
        ls.NewColumn();
        ls.End();
    }
}