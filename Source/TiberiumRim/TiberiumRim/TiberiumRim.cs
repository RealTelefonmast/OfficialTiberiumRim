using HarmonyLib;
using System.Reflection;
using UnityEngine;
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using RimWorld.BaseGen;
using Verse;
using Verse.AI.Group;
using Verse.Sound;
using System.Runtime.CompilerServices;
using System.IO;
using System.Threading;

namespace TiberiumRim
{
    public static class TiberiumRimSettings
    {
        public static TiberiumSettings settings;
    }

    public class TiberiumRimMod : Mod
    {
        public TiberiumSettings settings;
        public static TiberiumRimMod mod;
        public static AssetBundle assetBundle;
        private static Harmony tiberium;

        public static Harmony Tiberium => tiberium ??= new Harmony("com.tiberiumrim.rimworld.mod");

        public TiberiumRimMod(ModContentPack content) : base(content)
        {
            settings = GetSettings<TiberiumSettings>();
            TiberiumRimSettings.settings = settings;
            Tiberium.PatchAll(Assembly.GetExecutingAssembly());
            mod = this;
            Log.Message("[TiberiumRim] Loaded");
        }

        public void LoadAssetBundles()
        {
            string path = Path.Combine(Content.RootDir, @"Materials\Shaders\shaderbundle");
            assetBundle = AssetBundle.LoadFromFile(path);
            TiberiumContent.AlphaShader = (Shader)assetBundle.LoadAsset("AlphaShader");
            TiberiumContent.AlphaShaderMaterial = (Material)assetBundle.LoadAsset("ShaderMaterial");
        }

        public void PatchPawnDefs()
        {
            foreach (var def in DefDatabase<ThingDef>.AllDefs)
            {
                if(def?.thingClass == null) continue;
                Type thingClass = def.thingClass;
                if (!thingClass.IsSubclassOf(typeof(Pawn)) && thingClass != typeof(Pawn)) continue;
                if(def.comps == null)
                    def.comps = new List<CompProperties>();
                def.comps.Add(new CompProperties_TiberiumCheck());
                def.comps.Add(new CompProperties_CrystalDrawer());
            }
        }

        /*
        [HarmonyPatch(typeof(ModContentLoader<Texture2D>))]
        [HarmonyPatch("LoadPNG")]
        public static class LoadPNGPatch
        {
            static bool Prefix(string filePath, ref Texture2D __result)
            {
                
                if (filePath.Contains("\\TiberiumRim\\Textures\\"))
                {
                    Texture2D texture2D = null;
                    if (File.Exists(filePath))
                    {
                        bool mipmap = filePath.Contains("\\UI\\");
                        byte[] data = File.ReadAllBytes(filePath);
                        texture2D = new Texture2D(2, 2, TextureFormat.Alpha8, true);
                        texture2D.anisoLevel = 9;
                        //texture2D.mipMapBias = -0.5f;
                        texture2D.LoadImage(data);
                        texture2D.name = Path.GetFileNameWithoutExtension(filePath);
                        texture2D.filterMode = FilterMode.Trilinear;
                        texture2D.Apply(true, true);
                    }
                    __result = texture2D;
                    return false;
                }         
                
                return true;
            }
        }
        */

        [HarmonyPatch(typeof(DefGenerator))]
        [HarmonyPatch("GenerateImpliedDefs_PreResolve")]
        public static class GenerateImpliedDefs_PreResolvePatch
        {
            public static void Postfix()
            {
                //Log.Message("Patching " + DefDatabase<TRThingDef>.AllDefs.Count() + " items");
                foreach (TRThingDef def in DefDatabase<TRThingDef>.AllDefs)
                {
                    if (def.drawerType == DrawerType.MapMeshOnly && def.comps.Any(c => c is CompProperties_FX fx && fx.overlays.Any(o => o.mode != FXMode.Static)))
                        Log.Warning(def + " has dynamic overlays but is MapMeshOnly");
                    if (def.factionDesignation == null && !def.needsBlueprint) continue;
                    TRThingDefList.Add(def);
                    ThingDef blueprint = TRUtils.MakeNewBluePrint(def, false, null);
                    ThingDef frame = TRUtils.MakeNewFrame(def);
                    DefGenerator.AddImpliedDef(blueprint);
                    DefGenerator.AddImpliedDef(frame);
                    if (def.Minifiable)
                    {
                        ThingDef mini = TRUtils.MakeNewBluePrint(def, true, blueprint);
                    }
                    DirectXmlCrossRefLoader.ResolveAllWantedCrossReferences(FailMode.Silent);
                }
                Log.Message("TRThingDefList - Faction Cats: " + TRThingDefList.Categorized.Keys.Count + " | TRThings: " + TRThingDefList.TotalCount);
            }
        }
    }
}
