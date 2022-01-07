using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class TiberiumRimMod : Mod
    {
        //Static Data
        public static TiberiumRimMod mod;
        private static Harmony tiberium;

        //
        public TiberiumSettings settings;
        public static bool isDebug = true;

        public static Harmony Tiberium => tiberium ??= new Harmony("telefonmast.tiberiumrim");

        public AssetBundle MainBundle
        {
            get
            {
                string pathPart = "";
                if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    pathPart = "StandaloneOSX";
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    pathPart = "StandaloneWindows";
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    pathPart = "StandaloneLinux64";

                string mainBundlePath = Path.Combine(Content.RootDir, $@"Materials\Bundles\{pathPart}\tiberiumrimbundle");
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

        public TiberiumRimMod(ModContentPack content) : base(content)
        {
            mod = this;

            Log.Message("[TiberiumRim] - Init");
            settings = GetSettings<TiberiumSettings>();

            //
            Tiberium.PatchAll(Assembly.GetExecutingAssembly());

            //
            
        }

        public override void WriteSettings()
        {
            base.WriteSettings();
        }

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

        public void PatchPawnDefs()
        {
            foreach (var def in DefDatabase<ThingDef>.AllDefs)
            {
                if (def?.thingClass == null) continue;
                Type thingClass = def.thingClass;
                if (!thingClass.IsSubclassOf(typeof(Pawn)) && thingClass != typeof(Pawn)) continue;
                if (def.comps == null)
                    def.comps = new List<CompProperties>();
                def.comps.Add(new CompProperties_TiberiumCheck());
                def.comps.Add(new CompProperties_PawnExtraDrawer());
                def.comps.Add(new CompProperties_CrystalDrawer());
            }
        }

        [HarmonyPatch(typeof(DefGenerator))]
        [HarmonyPatch("GenerateImpliedDefs_PreResolve")]
        public static class GenerateImpliedDefs_PreResolvePatch
        {
            public static void Postfix()
            {
                foreach (TRThingDef def in DefDatabase<TRThingDef>.AllDefs)
                {
                    if (def.drawerType == DrawerType.MapMeshOnly && def.comps.Any(c => c is CompProperties_FX fx && fx.overlays.Any(o => o.mode != FXMode.Static)))
                        Log.Warning(def + " has dynamic overlays but is MapMeshOnly");
                    if (def.factionDesignation == null) continue;
                    TRThingDefList.Add(def);
                    ThingDef blueprint = TRUtils.MakeNewBluePrint(def, false, null);
                    ThingDef frame = TRUtils.MakeNewFrame(def);
                    DefGenerator.AddImpliedDef(blueprint);
                    DefGenerator.AddImpliedDef(frame);
                    if (def.Minifiable)
                    {
                        def.minifiedDef = TRUtils.MakeNewBluePrint(def, true, blueprint);
                    }
                    DirectXmlCrossRefLoader.ResolveAllWantedCrossReferences(FailMode.Silent);
                }
            }
        }
    }
}
