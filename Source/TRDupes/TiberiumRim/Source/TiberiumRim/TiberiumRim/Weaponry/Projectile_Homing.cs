using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using UnityEngine;

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
