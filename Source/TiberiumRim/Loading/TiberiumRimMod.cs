using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using TeleCore.ThingComps.Props;
using TeleCore.VFX.FX.Layer.Properties;
using TR.Defs;
using TR.GameParts;
using TR.Hediffs.TiberiumInfection;
using TR.Rendering.TextureContent;
using TR.Util;
using UnityEngine;
using Verse;

namespace TR.Loading;

public class TiberiumRimMod : Mod
{
    //Static Data
    public static TiberiumRimMod mod;
    public static AssetBundle assetBundle;
    private static Harmony tiberium;

    //
    public TiberiumSettings settings;

    public TiberiumRimMod(ModContentPack content) : base(content)
    {
        Log.Message("[TiberiumRim] - Init");
        settings = GetSettings<TiberiumSettings>();

        Tiberium.PatchAll(Assembly.GetExecutingAssembly());
        mod = this;
    }

    public static Harmony Tiberium => tiberium ??= new Harmony("com.tiberiumrim.rimworld.mod");

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
            def.comps.Add(new CompProperties_PawnExtraDrawer());
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