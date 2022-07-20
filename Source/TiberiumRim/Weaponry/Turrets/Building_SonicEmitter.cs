using TeleCore;
using UnityEngine;

namespace TiberiumRim
{
    public class Building_SonicEmitter : Building_TeleTurret
    {
        //FX
        public override bool FX_AffectsLayerAt(int index)
        {
            return index is 0;
        }

        public override Vector3? FX_GetDrawPositionAt(int index) => base.DrawPos;
        public override Color? FX_GetColorAt(int index) => Color.white;
        public override float? FX_GetRotationAt(int index) => MainGun.TurretRotation;
        public override float FX_GetOpacityAt(int index)
        {
            return index switch
            {
                _ => 1f
            };
        }

        public override bool FX_ShouldDrawAt(int index)
        {
            return index switch
            {
                _ => true
            };
        }
    }
}
