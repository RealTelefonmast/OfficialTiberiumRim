using System.Collections.Generic;
using Verse;

namespace TeleCore.Unsorted;

public class TeleCoreSettings : ModSettings
{
    public bool enableProjectileGraphicRandomFix;
    public bool showToolsInMainMenu;
    
    //
    private List<string> keyList;

    //Tools.General
    //internal Dictionary<string, ScribeDictionary<string, bool>> DataBrowserSettings = new();

    //Tools.Animation
    private string userDefinedAnimationDefLocation;
    private List<Dictionary<string, bool>> valueList;

    //Properties
    public string SaveAnimationDefLocation => userDefinedAnimationDefLocation;
    public bool ProjectileGraphicRandomFix => enableProjectileGraphicRandomFix;

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

    internal void SetAnimationDefLocation(string newPath, bool write = true)
    {
        userDefinedAnimationDefLocation = newPath;
        if (write)
            Write();
    }

    internal void ResetAnimationDefLocation()
    {
        SetAnimationDefLocation(RWStringCache.DefaultAnimationDefLocation);
    }

    public override void ExposeData()
    {
        Scribe_Values.Look(ref enableProjectileGraphicRandomFix, "enableProjectileGraphicRandomFix");
        Scribe_Values.Look(ref userDefinedAnimationDefLocation, "userDefinedAnimationDefLocation");
        Scribe_Values.Look(ref showToolsInMainMenu, "showToolsInMainMenu");
        //Scribe_Collections.Look(ref DataBrowserSettings, "DataBrowserSettings", LookMode.Value, LookMode.Deep);

        if (userDefinedAnimationDefLocation == null)
            SetAnimationDefLocation(RWStringCache.DefaultAnimationDefLocation, false);

        /*
        if (DataBrowserSettings == null)
        {
            DataBrowserSettings = new Dictionary<string, ScribeDictionary<string, bool>>();
        }
        */
    }
}