using System;
using System.Collections.Generic;
using RimWorld;
using TeleCore.CompProperties;
using TeleCore.Unsorted;
using UnityEngine;
using Verse;

namespace TeleCore.Comps;

public struct FXParentInfo
{
    public int TickOffset { get; }
    public int SpawnTick { get; }
    public FXDefExtension Extension { get; }
    public Thing ParentThing { get; }

    //
    public ThingDef Def => ParentThing.def;

    public FXParentInfo(int tickOffset, int spawnTick, FXDefExtension extension, Thing parentThing)
    {
        TickOffset = tickOffset;
        SpawnTick = spawnTick;
        Extension = extension;
        ParentThing = parentThing;
    }
}

public class CompFX : TeleComp
{
    private bool _hasEffecters;
    private bool _hasFXLayers;
    private List<IFXLayerProvider> allHeldFXComps;
    private IFXEffecterProvider[] EffecterProviderByLayerIndex;

    //Events
    /*
    private FXGetPowerProviderEvent GetPowerProvider;
    private FXGetShouldDrawEvent GetShouldDraw;
    private FXGetRotationEvent GetRotation;
    private FXGetAnimationSpeedEvent GetAnimationSpeed;
    private FXGetDrawPositionEvent GetDrawPosition;
    private FXGetColorEvent GetColor;
    private FXGetOpacityEvent GetOpacity;
    private FXGeActionEvent GetAction;
    private FXShouldThrowEffectsEvent GetShouldThrowEffects;
    */

    private OnEffectSpawnedEvent EffectSpawned;

    private IFXLayerProvider[] LayerProviderByLayerIndex;
    private bool spawnedOnce;

    public CompProperties_FX Props => (CompProperties_FX)props;

    public CompPowerTrader ParentPowerComp { get; private set; }
    public FXDefExtension GraphicExtension { get; private set; }
    public List<FXLayer> FXLayers { get; private set; }
    public List<EffecterLayer> EffectLayers { get; private set; }

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);
        GraphicExtension = parent.def.FXExtension();

        ParentPowerComp = parent.TryGetComp<CompPowerTrader>();

        //Resolve FX Parents
        ResolveFXLayerProviders();
        ResolveFXEffecterProviders();

        //Setup Layers
        //Init Data On First Spawn
        if (!spawnedOnce || respawningAfterLoad)
        {
            //Generate FXLayers
            if (!Props.fxLayers.NullOrEmpty())
            {
                FXLayers = new List<FXLayer>();
                var spawnTick = Find.TickManager.TicksGame;
                var tickOffset = Props.tickOffset.RandomInRange;

                for (var i = 0; i < Props.fxLayers.Count; i++)
                    FXLayers.Add(new FXLayer(this, Props.fxLayers[i],
                        new FXParentInfo(tickOffset, spawnTick, GraphicExtension, parent), i));

                //Resolve priority
                FXLayers.Sort((a, b) => a.RenderPriority < b.RenderPriority ? 1 : 0);
                _hasFXLayers = FXLayers?.Count > 0;
            }

            //Generate FXEffecters
            if (!Props.effectLayers.NullOrEmpty())
            {
                EffectLayers = new List<EffecterLayer>();
                for (var i = 0; i < Props.effectLayers.Count; i++)
                    EffectLayers.Add(new EffecterLayer(this, Props.effectLayers[i], i));

                //
                _hasEffecters = EffectLayers?.Count > 0;
            }

            spawnedOnce = true;
        }
    }

    public override void PostExposeData()
    {
        base.PostExposeData();
    }

    private void ResolveFXLayerProviders()
    {
        allHeldFXComps ??= new List<IFXLayerProvider>();

        //Add parent if it implements the FX interface
        if (parent is IFXLayerProvider parentFX)
        {
            allHeldFXComps.Add(parentFX);
            PopulateEvents(parentFX);
        }

        //Populate Events
        foreach (var comp in parent.AllComps)
            if (comp is IFXLayerProvider compFX)
            {
                allHeldFXComps.Add(compFX);
                PopulateEvents(compFX);
            }

        //Resolve
        LayerProviderByLayerIndex = new IFXLayerProvider[Props.fxLayers.Count];
        for (var i = 0; i < Props.fxLayers.Count; i++)
        {
            var layerData = Props.fxLayers[i];
            if (allHeldFXComps.NullOrEmpty())
            {
                LayerProviderByLayerIndex[i] = null!;
                continue;
            }

            LayerProviderByLayerIndex[i] = allHeldFXComps.FirstOrFallback(fx =>
            {
                return (bool)fx?.FX_ProvidesForLayer(new FXArgs
                {
                    index = i,
                    layerTag = layerData.layerTag,
                    categoryTag = layerData.categoryTag
                });
            })!;
        }
    }

    private void ResolveFXEffecterProviders()
    {
        var effectProviders = new List<IFXEffecterProvider>();

        //Add parent if it implements the FX interface
        if (parent is IFXEffecterProvider parentFX)
        {
            effectProviders.Add(parentFX);
            PopulateEvents(parentFX);
        }

        //Populate Events
        foreach (var comp in parent.AllComps)
            if (comp is IFXEffecterProvider compFX)
            {
                effectProviders.Add(compFX);
                PopulateEvents(compFX);
            }

        //Resolve
        EffecterProviderByLayerIndex = new IFXEffecterProvider[Props.fxLayers.Count];
        for (var i = 0; i < Props.effectLayers.Count; i++)
        {
            var effectData = Props.effectLayers[i];
            if (allHeldFXComps.NullOrEmpty())
            {
                LayerProviderByLayerIndex[i] = null!;
                continue;
            }

            LayerProviderByLayerIndex[i] = allHeldFXComps.FirstOrFallback(fx => (bool)fx?.FX_ProvidesForLayer(
                new FXEffecterArgs
                {
                    index = i,
                    layerTag = effectData.layerTag
                }))!;
        }
    }

    private void PopulateEvents(IFXBase fxProvider)
    {
        // GetPowerProvider += fxHolder.FX_PowerProviderFor;
        // GetShouldDraw += fxHolder.FX_ShouldDraw;
        // GetOpacity += fxHolder.FX_GetOpacity;
        // GetRotation += fxHolder.FX_GetRotation;
        // GetAnimationSpeed += fxHolder.FX_GetAnimationSpeedFactor;
        // GetColor += fxHolder.FX_GetColor;
        // GetDrawPosition += fxHolder.FX_GetDrawPosition;
        // GetAction += fxHolder.FX_GetAction;
        //
        // GetShouldThrowEffects += fxHolder.FX_ShouldThrowEffects;

        if (fxProvider is IFXEffecterProvider effectProvider)
            EffectSpawned += effectProvider.FX_OnEffectSpawned;
    }

    //Notification
    public override void ReceiveCompSignal(string signal)
    {
        if (!parent.Spawned) return;
        if (signal is "PowerTurnedOn" or "PowerTurnedOff" or "FlickedOn" or "FlickedOff" or "Refueled" or "RanOutOfFuel"
            or "ScheduledOn" or "ScheduledOff")
            parent.Map.mapDrawer.MapMeshDirty(parent.Position, MapMeshFlagDefOf.Things);
    }

    internal override void TeleTick()
    {
        if (parent.def.tickerType == TickerType.Never) FXTick(1);
    }

    public override void CompTick()
    {
        FXTick(1);
    }

    public override void CompTickRare()
    {
        for (var i = 0; i < GenTicks.TickRareInterval; i++)
            FXTick(GenTicks.TickRareInterval);
    }

    private void FXTick(int tickInterval)
    {
        if (_hasFXLayers)
            foreach (var g in FXLayers)
                g.TickLayer(tickInterval);

        if (_hasEffecters)
            foreach (var effectLayer in EffectLayers)
                effectLayer.Tick();
    }

    //Drawing
    private bool CanDraw(FXLayerArgs args)
    {
        if (args.data.skip) return false;
        if (!GetDrawBool(args)) return false;
        if (GetOpacityFloat(args) <= 0) return false;
        if (!HasPower(args)) return false;
        return true;
    }

    public bool HasPower(FXArgs args)
    {
        if (!args.needsPower) return true;

        var provider = GetPowerProvider(args);

        if (provider is CompPowerPlant powerPlant)
        {
            _ = powerPlant.PowerOn;
            return powerPlant.PowerOutput > 0;
        }

        if (provider is { PowerOn: true }) return true;

        return ParentPowerComp is { PowerOn: true };
    }

    #region Base Properties

    //
    public CompPowerTrader GetPowerProvider(FXArgs args)
    {
        return LayerProviderByLayerIndex[args.index]?.FX_PowerProviderFor(args) ?? ParentPowerComp;
    }

    #endregion

    //
    public void DrawCarried(Vector3 loc)
    {
        foreach (var layer in FXLayers)
            if (layer.data.fxMode != FXMode.Static && CanDraw(layer.Args))
            {
                var drawPos = GetDrawPositionOverride(layer.Args);
                var diff = drawPos - parent.DrawPos;
                layer.Draw(loc + diff);
            }
    }

    public override void PostDraw()
    {
        base.PostDraw();
        if (!_hasFXLayers) return;

        foreach (var layer in FXLayers)
        {
            var canDraw = CanDraw(layer.Args);
            if (layer.data.fxMode != FXMode.Static && canDraw)
                layer.Draw();
        }
    }

    public override void PostPrintOnto(SectionLayer layer)
    {
        base.PostPrintOnto(layer);
        if (!_hasFXLayers) return;
        foreach (var fxLayer in FXLayers)
            if (fxLayer.data.fxMode == FXMode.Static && CanDraw(fxLayer.Args))
                fxLayer.Print(layer);
    }

    public override IEnumerable<Gizmo> CompGetGizmosExtra()
    {
        if (DebugSettings.godMode)
        {
            var states = "";
            foreach (var layer in FXLayers)
            {
                states += $"Layer {layer.Args.index} '{layer.Args.categoryTag}:{layer.Args.layerTag}':\n";
                if (layer.data.skip)
                {
                    states += "  - Skipped\n";
                    continue;
                }

                states += $"  - FXMode: {layer.data.fxMode}\n";
                var drawBool = GetDrawBool(layer.Args);
                var power = HasPower(layer.Args);
                var opacity = GetOpacityFloat(layer.Args);
                states += $"  - Draw: {drawBool} && {power} && {opacity > 0}\n";
                states += $"  - Opacity: {opacity}\n";
                states += $"  - Rotation: {GetExtraRotation(layer.Args)}\n";
                states += $"  - Animation Speed: {GetAnimationSpeedFactor(layer.Args)}\n";
                states += $"  - Color: {GetColorOverride(layer.Args)?.ToString() ?? "Default"}\n";
                states += $"  - Draw Position: {GetDrawPositionOverride(layer.Args)}\n";
                states += $"  - Action: {GetDrawFunction(layer.Args)?.ToString() ?? "None"}\n";
            }

            yield return new Command_Action
            {
                defaultLabel = "FX States",
                defaultDesc = states,
                action = delegate { }
            };
        }
    }

    #region Effecter Properties

    //
    public TargetInfo TargetAOverride(FXEffecterArgs args)
    {
        return EffecterProviderByLayerIndex[args.index]?.FX_Effecter_TargetAOverride(args) ?? parent;
    }

    //
    public TargetInfo TargetBOverride(FXEffecterArgs args)
    {
        return EffecterProviderByLayerIndex[args.index]?.FX_Effecter_TargetBOverride(args) ?? parent;
    }

    public bool ShouldThrowEffects(FXEffecterArgs args)
    {
        return EffecterProviderByLayerIndex[args.index]?.FX_ShouldThrowEffects(args) ?? true;
    }

    //
    public void OnEffectSpawned(FXEffecterSpawnedEventArgs spawnedEventArgs)
    {
        EffectSpawned.Invoke(spawnedEventArgs);
    }

    #endregion

    #region Layer Properties

    public bool GetDrawBool(FXLayerArgs args)
    {
        return LayerProviderByLayerIndex[args.index]?.FX_ShouldDraw(args) ?? true;
        //return GetShouldDraw.Invoke(args);
    }

    public float GetOpacityFloat(FXLayerArgs args)
    {
        return LayerProviderByLayerIndex[args.index]?.FX_GetOpacity(args) ?? 1f;
    }

    public float? GetRotationSpeedOverride(FXLayerArgs args)
    {
        return LayerProviderByLayerIndex[args.index]?.FX_GetRotationSpeedOverride(args);
    }

    public float GetExtraRotation(FXLayerArgs args)
    {
        return LayerProviderByLayerIndex[args.index]?.FX_GetRotation(args) ?? 0;
    }

    public float GetAnimationSpeedFactor(FXLayerArgs args)
    {
        return LayerProviderByLayerIndex[args.index]?.FX_GetAnimationSpeedFactor(args) ?? 1;
    }

    public int GetSelectedIndex(FXLayerArgs args)
    {
        return LayerProviderByLayerIndex[args.index]?.FX_SelectedGraphicIndex(args) ?? 0;
    }

    public Color? GetColorOverride(FXLayerArgs args)
    {
        return LayerProviderByLayerIndex[args.index]?.FX_GetColor(args) ?? null;
    }

    public Vector3? GetDrawPositionOverride(FXLayerArgs args)
    {
        return LayerProviderByLayerIndex[args.index]?.FX_GetDrawPosition(args) ?? null;
        //return GetDrawPosition.Invoke(args);
    }

    public Func<RoutedDrawArgs, bool> GetDrawFunction(FXLayerArgs args)
    {
        return LayerProviderByLayerIndex[args.index]?.FX_GetDrawFunc(args) ?? null!;
        //return GetAction.Invoke(args);
    }

    #endregion
}