using System;
using RimWorld;
using TeleCore.Events;
using TeleCore.Rendering;
using TR.Rendering.TextureContent;
using UnityEngine;
using Verse;

namespace TR.Weaponry;

public class Building_Obelisk : Building_TRTurret
{
    private float chargeAmount;

    public float ObeliskCharge =>
        Mathf.InverseLerp(0, MainGun.props.turretBurstWarmupTime.SecondsToTicks(), chargeAmount);

    public override LocalTargetInfo CurrentTarget => MainGun.CurrentTarget;

    public override ExtendedGraphicData ExtraData => def.extraData;
/*
    public override Vector3[] DrawPositions => [base.DrawPos, base.DrawPos];
    public override Color[] ColorOverrides => [Color.white, Color.white];
    public override float[] OpacityFloats => [1f, ObeliskCharge];
    public override float?[] RotationOverrides => [null, null];
    public override bool[] DrawBools => [true, chargeAmount > 0];
    public override bool ShouldDoEffecters => true;
*/
    public override void Tick()
    {
        base.Tick();
        if (CurrentTarget.IsValid)
        {
            if (MainGun.burstWarmupTicksLeft > 0)
                chargeAmount++;
        }
        else if (chargeAmount > 0)
        {
            chargeAmount--;
        }
    }

    public override void DrawAt(Vector3 drawLoc, bool flip = false)
    {
        if (args.layerTag == "FXObelisk")
            return true;
        return base.FX_ProvidesForLayer(args);
    }
    
    public override float? FX_GetOpacity(FXLayerArgs args)
    {
        return args.index switch
        {
            1 => ObeliskCharge,
            _ => 1f
        };
    }

    public override bool? FX_ShouldDraw(FXLayerArgs args)
    {
        return args.index switch
        {
            1 => chargeAmount > 0,
            _ => true
        };
    }

    public override void Draw()
    {
        base.Draw();
        if (CurrentTarget.IsValid && CurrentTarget.Thing is Pawn p)
        {
            DrawMarkedForDeath(p);
    }
    
    private static void DrawMarkedForDeath(Pawn target)
    {
        var mat = MaterialPool.MatFrom(TiberiumContent.MarkedForDeath, ShaderDatabase.MetaOverlay, Color.white);
        var num = (Time.realtimeSinceStartup + 397f * (target.thingIDNumber % 571)) * 4f;
        var num2 = ((float)Math.Sin(num) + 1f) * 0.5f;
        num2 = 0.3f + num2 * 0.7f;
        var material = FadedMaterialPool.FadedVersionOf(mat, num2);
        var c = target.TrueCenter() + new Vector3(0, 0, 1.15f);
        UnityEngine.Graphics.DrawMesh(MeshPool.plane08, new Vector3(c.x, AltitudeLayer.MetaOverlays.AltitudeFor(), c.z),
            Quaternion.identity, material, 0);
    }
}