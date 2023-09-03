using TeleCore;
using TeleCore.Data.Events;
using UnityEngine;

namespace TR
{
    public class Building_NodHubTurret : Building_HubTurret
    {
        //FX
        public override Vector3? FX_GetDrawPosition(FXLayerArgs args)
        {
            return args.index switch
            {
                0 => MainGun.DrawPos,                   //Lights
                1 => null,                              //Skip
                2 => MainGun.DrawPos,                   //GlowNoBaarel
                3 => MainGun.Top.Barrels[0].DrawPos,    //Glow Left
                4 => MainGun.Top.Barrels[1].DrawPos,    //Glow Right
                _ => null
            };
        }

        public override float? FX_GetRotation(FXLayerArgs args)
        {
            return args.index switch
            {
                2 => MainGun?.TurretRotation,
                3 => MainGun?.TurretRotation,
                4 => MainGun?.TurretRotation,
                _ => null
            };
        }
    }
}
