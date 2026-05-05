using System;
using HarmonyLib;
using RimWorld;
using TeleCore.Comps;
using TeleCore.Rendering;
using UnityEngine;
using Verse;

namespace TeleCore.Unsorted;

/// <summary>
///     <para>
///         Implementing this on a <see cref="ThingComp" /> or <see cref="Thing" /> allows you to affect the layers in a
///         <see cref="CompFX" /> attached to the same Thing instance.
///     </para>
///     <para>
///         You can implement this interface on multiple parts of a Thing instance, including the base
///         <see cref="ThingDef.thingClass" /> and the <see cref="ThingDef.comps" />.
///     </para>
///     <para>
///         If multiple implementations are active, the order of priority for selecting an interface for a layer via
///         <see cref="FX_AffectsLayerAt" /> or for <see cref="IsMain" /> is done by <see cref="Priority" />.
///     </para>
/// </summary>

// RenderPriority
// #0  => Power Glow
// #8  => Network Glow
// #16 => Network Container Fill
// #24 => Network "Lid"
// #32 =>

//TODO: FX RE-STRUCTURE
//[Base]
//  []
//
//

//[FXArgs]  -> [FXEffecterArgs]
//          -> [FXLayerArgs]
public struct FXProviderState
{
    public bool IsLayerProvider;
    public CompPowerTrader PowerProvider;
}

public struct EffecterState
{
    public bool ShouldThrowEffects;
    public TargetInfo TargetA;
    public TargetInfo TargetB;
    public Action<FXEffecterSpawnedEventArgs> EffectSpawnedAction;
}

public struct FXState
{
    public int SelectedGraphicIndex;
    public bool ShouldDraw;
    public float Opacity;
    public float Rotation;
    public float RotationSpeedOverride;
    public float AnimationSpeedFactor;
    public Color Color;
    public Vector3 DrawPosition;
    public Func<RoutedDrawArgs, bool> DrawFunc;
}

public interface IFXBase
{
    #region MetaDataGetter

    /// <summary>
    /// </summary>
    bool FX_ProvidesForLayer(FXArgs args);

    /// <summary>
    ///     Allows you to override the default power getter with a custom reference, otherwise it defaults to the parent
    ///     Thing's PowerComp (if it exists)
    /// </summary>
    CompPowerTrader FX_PowerProviderFor(FXArgs args);

    #endregion
}

public interface IFXEffecterProvider : IFXBase
{
    /// <summary>
    ///     Sets whether or not an attached Comp_FleckThrower should throw effects.
    /// </summary>
    bool? FX_ShouldThrowEffects(FXEffecterArgs args);

    public TargetInfo FX_Effecter_TargetAOverride(FXEffecterArgs args);
    public TargetInfo FX_Effecter_TargetBOverride(FXEffecterArgs args);

    /// <summary>
    ///     Allows you to hook into the effecter logic, and handle custom logic whenever a tagged effect is spawned.
    /// </summary>
    void FX_OnEffectSpawned(FXEffecterSpawnedEventArgs args);
}

public interface IFXLayerProvider : IFXBase
{
    /// <summary>
    ///     Sets the index of the graphic to choose for Graphic_Selectable layers.
    /// </summary>
    int? FX_SelectedGraphicIndex(FXLayerArgs args);

    /// <summary>
    ///     Overrides whether a layer at the same index of that value is rendered or not.
    /// </summary>
    bool? FX_ShouldDraw(FXLayerArgs args);

    /// <summary>
    ///     Sets the opacity value of a layer at the same index as the value in the array.
    /// </summary>
    float? FX_GetOpacity(FXLayerArgs args);

    /// <summary>
    ///     Sets the rotation value of a layer at the same index as the value in the array.
    /// </summary>
    float? FX_GetRotation(FXLayerArgs args);

    /// <summary>
    /// </summary>
    float? FX_GetRotationSpeedOverride(FXLayerArgs args);

    /// <summary>
    ///     Sets the speed at which the layer processes dynamic images (rotating, blinking, moving)
    /// </summary>
    float? FX_GetAnimationSpeedFactor(FXLayerArgs args);

    /// <summary>
    ///     Overrides the draw color of the layer at the index of the value.
    /// </summary>
    Color? FX_GetColor(FXLayerArgs args);

    /// <summary>
    ///     Sets the exact render position of the layer at the index of that value.
    /// </summary>
    Vector3? FX_GetDrawPosition(FXLayerArgs args);

    /// <summary>
    ///     Attaches a custom function to a layer, it is run before the layer is drawn.
    ///     Returns a bool which defines where the actual layer draw function should be run.
    /// </summary>
    Func<RoutedDrawArgs, bool> FX_GetDrawFunc(FXLayerArgs args);

    /*
    #region Effecters

    /// <summary>
    /// Sets whether or not an attached Comp_FleckThrower should throw effects.
    /// </summary>
    bool? FX_ShouldThrowEffects(FXLayerArgs args);

    /// <summary>
    /// Allows you to hook into the effecter logic, and handle custom logic whenever a tagged effect is spawned.
    /// </summary>
    void FX_OnEffectSpawned(FXEffecterSpawnedEffectEventArgs args);

    #endregion
    */
}