using System.Collections.Generic;
using System.IO;
using TeleCore.Loader.Configs;
using Verse;

namespace TeleCore.Loader;

public class TeleCoreSettings : ModSettings
{
    public bool showToolsInMainMenu;
    public AnimationConfig animationConfig;
    public FixesConfig fixesConfig;
    public PatchConfig patchConfig;

    //
    private List<string> keyList;

    //Tools.General
    //internal Dictionary<string, ScribeDictionary<string, bool>> DataBrowserSettings = new();

    //Properties
    public string SaveAnimationDefLocation => animationConfig.userDefinedAnimationDefLocation;
    public bool ProjectileGraphicRandomFix => fixesConfig.enableProjectileGraphicRandomFix;

    //Data Notifiers
    /*
    internal bool AllowsModInDataBrowser(Type forType, ModContentPack mod)
    {
        if (!DataBrowserSettings.TryGetValue(forType.ToString(), out var settings)) return true;
        return !settings.TryGetValue(mod.Name, out var value) || value;
    }

    internal void SetDataBrowserSettings(Type forType, string packName, bool value)
    {
        if (!DataBrowserSettings.TryGetValue(forType.ToString(), out var settings))
        {
            settings = new ScribeDictionary<string, bool>(LookMode.Value, LookMode.Value);
            DataBrowserSettings.Add(forType.ToString(), settings);
        }
        if (!settings.ContainsKey(packName))
        {
            settings.Add(packName, value);
            return;
        }
        settings[packName] = value;
        Write();
    }
    */

    public override void ExposeData()
    {
        Scribe_Values.Look(ref showToolsInMainMenu, "showToolsInMainMenu");
        Scribe_Deep.Look(ref animationConfig, "animationConfig", this);
        Scribe_Deep.Look(ref fixesConfig, "fixesConfig");
        Scribe_Deep.Look(ref patchConfig, "patchConfig");
        //Scribe_Collections.Look(ref DataBrowserSettings, "DataBrowserSettings", LookMode.Value, LookMode.Deep);
        

        /*
        if (DataBrowserSettings == null)
        {
            DataBrowserSettings = new Dictionary<string, ScribeDictionary<string, bool>>();
        }
        */
    }
}