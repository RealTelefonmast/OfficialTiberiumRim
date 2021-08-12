using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class Projectile_Homing  : Projectile
    {
        public override void Tick()
        {
            base.Tick();
        }

        public override Vector3 ExactPosition { get; }
    }
}
