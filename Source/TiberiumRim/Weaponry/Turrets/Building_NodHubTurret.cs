using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeleCore;
using UnityEngine;

namespace TiberiumRim
{
    public class Building_NodHubTurret : Building_HubTurret
    {
        //FX
        public override Vector3? FX_GetDrawPositionAt(int index)
        {
            return index switch
            {
                0 => MainGun.DrawPos,                   //Lights
                1 => null,                              //Skip
                2 => MainGun.DrawPos,                   //GlowNoBaarel
                3 => MainGun.Top.Barrels[0].DrawPos,    //Glow Left
                4 => MainGun.Top.Barrels[1].DrawPos,    //Glow Right
                _ => null
            };
        }

        public override float? FX_GetRotationAt(int index)
        {
            return index switch
            {
                2 => MainGun?.TurretRotation,
                3 => MainGun?.TurretRotation,
                4 => MainGun?.TurretRotation,
                _ => null
            };
        }
    }
}
