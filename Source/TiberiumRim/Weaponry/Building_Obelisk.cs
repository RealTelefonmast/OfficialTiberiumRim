using System;
using RimWorld;
using TeleCore;
using UnityEngine;
using Verse;

namespace TiberiumRim;

public class Building_Obelisk : Building_TeleTurret
{
    private float chargeAmount = 0;

    private float ObeliskCharge =>
        Mathf.SmoothStep(0, 1,
            Mathf.InverseLerp(0, MainGun.Props.turretBurstWarmupTime.SecondsToTicks(), chargeAmount));

    public override LocalTargetInfo CurrentTarget => MainGun.CurrentTarget;

    public override void Tick()
    {
        base.Tick();
        if (CurrentTarget.IsValid)
        {
            if (MainGun.BurstWarmupTicksLeft > 0)
                chargeAmount++;
        }
        else if (chargeAmount > 0)
        {
            chargeAmount--;
        }
    }

    //FX
    public override bool FX_ProvidesForLayer(FXArgs args)
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
    }

    private void DrawMarkedForDeath(Pawn target)
    {
        Material mat = MaterialPool.MatFrom(TiberiumContent.MarkedForDeath, ShaderDatabase.MetaOverlay, Color.white);
        float num = (Time.realtimeSinceStartup + 397f * (float) (target.thingIDNumber % 571)) * 4f;
        float num2 = ((float) Math.Sin((double) num) + 1f) * 0.5f;
        num2 = 0.3f + num2 * 0.7f;
        Material material = FadedMaterialPool.FadedVersionOf(mat, num2);
        var c = target.TrueCenter() + new Vector3(0, 0, 1.15f);
        Graphics.DrawMesh(MeshPool.plane08, new Vector3(c.x, AltitudeLayer.MetaOverlays.AltitudeFor(), c.z),
            Quaternion.identity, material, 0);
    }
}

