using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using HarmonyLib;
using TeleCore.Logging;
using TR.Util;
using UnityEngine;
using Verse;

namespace TR;

public class TiberiumCoreMod : Mod, ILoggerProvider
{
    //Static Data
    public static TiberiumCoreMod mod;
    private static Harmony tiberium;

    //
    public static bool isDebug = true;

    public TiberiumCoreMod(ModContentPack content) : base(content)
    {
        mod = this;
        TRLog.Logger.DebugEnabled = () => isDebug;
        var version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        TRLog.Message($"[TiberiumRim][{version}] - Init", Color.cyan);
        modSettings = GetSettings<TiberiumCoreSettings>();

        Tiberium.PatchAll(Assembly.GetExecutingAssembly());
    }

    public static ModLogger Log => TRLog.Logger;

    public static Harmony Tiberium => tiberium ??= new Harmony("telefonmast.tiberiumrim.core");
    public static TiberiumCoreSettings CoreSettings => (TiberiumCoreSettings)mod.modSettings;

    public AssetBundle MainBundle
    {
        get
        {
            var pathPart = "";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                pathPart = "StandaloneOSX";
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                pathPart = "StandaloneWindows";
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                pathPart = "StandaloneLinux64";

            var mainBundlePath = Path.Combine(Content.RootDir, $@"Materials\Bundles\{pathPart}\tiberiumrimbundle");
            return AssetBundle.LoadFromFile(mainBundlePath);
        }
    }

    ModLogger ILoggerProvider.Log => TRLog.Logger;

    /*
PlatformID pid = System.Environment.OSVersion.Platform;
switch (pid)
{
case PlatformID.Win32NT:
case PlatformID.Win32S:
case PlatformID.Win32Windows:
case PlatformID.WinCE:
    Console.WriteLine("I'm on windows!");
    break;
case PlatformID.Unix:
    Console.WriteLine("I'm a linux box!");
    break;
case PlatformID.MacOSX:
    Console.WriteLine("I'm a mac!");
    break;
default:
    Console.WriteLine("No Idea what I'm on!");
    break;
}
*/

    /*
    public void LoadAssetBundles()
    {
        string mainBundlePath = Path.Combine(Content.RootDir, @"Materials\Shaders\tiberiumrimbundle");
        TRContentDatabase.SetBundle(AssetBundle.LoadFromFile(mainBundlePath));

        //string path = Path.Combine(Content.RootDir, @"Materials\Shaders\shaderbundle");
        //assetBundle = AssetBundle.LoadFromFile(path);
        //TiberiumContent.AlphaShader = (Shader)assetBundle.LoadAsset("AlphaShader");
        //TiberiumContent.AlphaShaderMaterial = (Material)assetBundle.LoadAsset("ShaderMaterial");
    }
    */
}