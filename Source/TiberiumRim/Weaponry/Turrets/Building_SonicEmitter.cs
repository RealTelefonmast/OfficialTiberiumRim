using TeleCore;
using TeleCore.Data.Events;

namespace TiberiumRim
{
    public class Building_SonicEmitter : Building_TeleTurret
    {
        //FX
        public override bool FX_ProvidesForLayer(FXArgs args)
        {
            if (args.layerTag == "FXSonicEmitter")
                return true;
            return base.FX_ProvidesForLayer(args);
        }
        
        public override float? FX_GetRotation(FXLayerArgs args) => MainGun.TurretRotation;
    }
}
