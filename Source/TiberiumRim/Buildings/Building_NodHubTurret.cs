using TeleCore.Events.Args;
using UnityEngine;

namespace TR;

public class Building_NodHubTurret : Building_HubTurret
{
    public override bool TransmitsPowerNow => PowerTraderComp.PowerOn;

    //FX
    public override bool FX_ProvidesForLayer(FXArgs args)
    {
        if (args.categoryTag == "PoweredLights")
            return true;
        return base.FX_ProvidesForLayer(args);
    }

    public override Vector3? FX_GetDrawPosition(FXLayerArgs args)
    {
        return args.index switch
        {
            0 => DrawPos,
            2 => MainGun.Top.Barrels[0].DrawPos, //Glow Left
            3 => MainGun.Top.Barrels[1].DrawPos, //Glow Right
            _ => MainGun.DrawPos
        };
    }

    public override float? FX_GetRotation(FXLayerArgs args)
    {
        return args.index switch
        {
            0 => null,
            _ => MainGun?.TurretRotation
        };
    }

    public override bool? FX_ShouldDraw(FXLayerArgs args)
    {
        if (args.categoryTag == "PoweredLights")
            return PowerTraderComp.PowerOn;

        return true;
    }
}