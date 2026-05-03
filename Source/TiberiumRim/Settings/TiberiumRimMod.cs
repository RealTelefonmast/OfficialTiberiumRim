using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using HarmonyLib;
using HugsLib.Utils;
using RimWorld;
using TiberiumRim;
using TR.TiberiumInfection;
using UnityEngine;
using Verse;

namespace TR;

public class TiberiumRimMod : Mod, ILoggerProvider
{
    public static TiberiumRimMod mod;
    public static AssetBundle assetBundle;
    private static Harmony tiberium;
    public static bool isDebug = true;
    public TiberiumSettings settings;

    public TiberiumRimMod(ModContentPack content) : base(content)
    {
        mod = this;
        TRLog.Logger.DebugEnabled = () => isDebug;
        var version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        TRLog.Message($"[TiberiumRim][{version}] - Init", Color.cyan);
        settings = GetSettings<TiberiumSettings>();
        Tiberium.PatchAll(Assembly.GetExecutingAssembly());
    }

    public static ModLogger Log => TRLog.Logger;
    public static Harmony Tiberium => tiberium ??= new Harmony("telefonmast.tiberiumrim.core");
    public static TiberiumSettings CoreSettings => (TiberiumSettings)mod.modSettings;

    ModLogger ILoggerProvider.Log => TRLog.Logger;

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

    public override void WriteSettings()
    {
        base.WriteSettings();
    }

    public void LoadAssetBundles()
    {
        var path = Path.Combine(Content.RootDir, @"Materials\Shaders\shaderbundle");
        assetBundle = AssetBundle.LoadFromFile(path);
        TiberiumContent.AlphaShader = (Shader)assetBundle.LoadAsset("AlphaShader");
        TiberiumContent.AlphaShaderMaterial = (Material)assetBundle.LoadAsset("ShaderMaterial");
    }

    public void PatchPawnDefs()
    {
        foreach (var def in DefDatabase<ThingDef>.AllDefs)
        {
            if (def?.thingClass == null) continue;
            var thingClass = def.thingClass;
            if (!thingClass.IsSubclassOf(typeof(Pawn)) && thingClass != typeof(Pawn)) continue;
            if (def.comps == null)
                def.comps = new List<CompProperties>();
            def.comps.Add(new CompProperties_TiberiumCheck());
            def.comps.Add(new TeleCore.Rendering.CompProperties_PawnExtraDrawer());
        }
    }

    [HarmonyPatch(typeof(DefGenerator))]
    [HarmonyPatch("GenerateImpliedDefs_PreResolve")]
    public static class GenerateImpliedDefs_PreResolvePatch
    {
        public static void Postfix()
        {
            foreach (var def in DefDatabase<TRThingDef>.AllDefs)
            {
                if (def.drawerType == DrawerType.MapMeshOnly && def.comps.Any(c =>
                        c is CompProperties_FX fx && fx.overlays.Any(o => o.mode != FXMode.Static)))
                    Log.Warning(def + " has dynamic overlays but is MapMeshOnly");
                if (def.factionDesignation == null) continue;
                TRThingDefList.Add(def);
                var blueprint = TRUtils.MakeNewBluePrint(def, false);
                var frame = TRUtils.MakeNewFrame(def);
                DefGenerator.AddImpliedDef(blueprint);
                DefGenerator.AddImpliedDef(frame);
                if (def.Minifiable) def.minifiedDef = TRUtils.MakeNewBluePrint(def, true, blueprint);
                DirectXmlCrossRefLoader.ResolveAllWantedCrossReferences(FailMode.Silent);
            }
        }
    }
}