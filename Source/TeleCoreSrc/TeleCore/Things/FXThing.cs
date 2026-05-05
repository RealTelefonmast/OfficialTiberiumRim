using System;
using RimWorld;
using TeleCore.Comps;
using TeleCore.Defs;
using TeleCore.Rendering;
using UnityEngine;
using Verse;

namespace TeleCore.Unsorted;

/// <summary>
///     A basic implementation of the <see cref="IFXLayerProvider" /> interface, uses <see cref="ThingWithComps" /> as a
///     base class.
/// </summary>
public class FXThing : ThingWithComps, IFXLayerProvider, IFXEffecterProvider
{
    public FXDefExtension Extension => def.FXExtension();
    public CompFX FXComp => GetComp<CompFX>();

    //
    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
    }

    public override void DrawAt(Vector3 drawLoc, bool flip = false)
    {
        base.DrawAt(drawLoc, flip);
        if (ParentHolder is Pawn)
            FXComp.DrawCarried(drawLoc);
    }


    #region FX Implementation

    //Basics
    public virtual bool FX_ProvidesForLayer(FXArgs args)
    {
        if (args.layerTag == "FXThing")
            return true;
        return false;
    }

    public virtual CompPowerTrader FX_PowerProviderFor(FXArgs args)
    {
        return null!;
    }

    //Layer
    public virtual bool? FX_ShouldDraw(FXLayerArgs args)
    {
        return null;
    }

    public virtual float? FX_GetOpacity(FXLayerArgs args)
    {
        return null;
    }

    public virtual float? FX_GetRotation(FXLayerArgs args)
    {
        return null;
    }

    public virtual float? FX_GetRotationSpeedOverride(FXLayerArgs args)
    {
        return null;
    }

    public virtual float? FX_GetAnimationSpeedFactor(FXLayerArgs args)
    {
        return null;
    }

    public virtual int? FX_SelectedGraphicIndex(FXLayerArgs args)
    {
        return null;
    }

    public virtual Color? FX_GetColor(FXLayerArgs args)
    {
        return null;
    }

    public virtual Vector3? FX_GetDrawPosition(FXLayerArgs args)
    {
        return null;
    }

    public virtual Func<RoutedDrawArgs, bool> FX_GetDrawFunc(FXLayerArgs args)
    {
        return null!;
    }

    //Effecters
    public virtual bool? FX_ShouldThrowEffects(FXEffecterArgs args)
    {
        return true;
    }

    public virtual TargetInfo FX_Effecter_TargetAOverride(FXEffecterArgs args)
    {
        return null;
    }

    public virtual TargetInfo FX_Effecter_TargetBOverride(FXEffecterArgs args)
    {
        return null;
    }

    public virtual void FX_OnEffectSpawned(FXEffecterSpawnedEventArgs args)
    {
    }

    #endregion
}