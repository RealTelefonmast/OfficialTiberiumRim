using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using HarmonyLib;
using TR.Util;
using UnityEngine;
using Verse;

namespace TR;

public class TRCoreMod : Mod
{
    //Static Data
    public static TRCoreMod mod;
    private static Harmony tiberium;

    //
    public static bool isDebug = true;


    public TRCoreMod(ModContentPack content) : base(content)
    {
        mod = this;
        var version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        TRLog.Message($"[TiberiumRim][{version}] - Init", Color.cyan);
        Tiberium.PatchAll(Assembly.GetExecutingAssembly());
    }

    public static Harmony Tiberium => tiberium ??= new Harmony("telefonmast.tiberiumrim.core");

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