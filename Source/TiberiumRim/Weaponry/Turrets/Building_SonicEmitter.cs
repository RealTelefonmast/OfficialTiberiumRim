using TeleCore;
using UnityEngine;

namespace TiberiumRim
{
    public class Building_SonicEmitter : Building_TeleTurret
    {
        //FX
        public override bool FX_ProvidesForLayer(FXArgs args)
        {
            return args.index is 0;
        }
        
        public override float? FX_GetRotation(FXLayerArgs args) => MainGun.TurretRotation;
    }
}
