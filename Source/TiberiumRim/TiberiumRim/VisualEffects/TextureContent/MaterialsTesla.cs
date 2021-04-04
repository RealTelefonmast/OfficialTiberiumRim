using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    [StaticConstructorOnStartup]
    public static class MaterialsTesla
    {
        public static Material[] Arcs = new[]
        {
            MaterialPool.MatFrom("Items/Weapons/Projectile/Electric/Arcs/arc1", ShaderDatabase.MoteGlow),
            MaterialPool.MatFrom("Items/Weapons/Projectile/Electric/Arcs/arc2", ShaderDatabase.MoteGlow),
            MaterialPool.MatFrom("Items/Weapons/Projectile/Electric/Arcs/arc3", ShaderDatabase.MoteGlow),
            MaterialPool.MatFrom("Items/Weapons/Projectile/Electric/Arcs/arc4", ShaderDatabase.MoteGlow)
        };

        public static Material[] Jumps = new[]
        {
            MaterialPool.MatFrom("Items/Weapons/Projectile/Electric/Jumps/arc1", ShaderDatabase.MoteGlow),
            MaterialPool.MatFrom("Items/Weapons/Projectile/Electric/Jumps/arc2", ShaderDatabase.MoteGlow),
            MaterialPool.MatFrom("Items/Weapons/Projectile/Electric/Jumps/arc3", ShaderDatabase.MoteGlow),
            MaterialPool.MatFrom("Items/Weapons/Projectile/Electric/Jumps/arc4", ShaderDatabase.MoteGlow),
            MaterialPool.MatFrom("Items/Weapons/Projectile/Electric/Jumps/arc5", ShaderDatabase.MoteGlow),
            MaterialPool.MatFrom("Items/Weapons/Projectile/Electric/Jumps/arc6", ShaderDatabase.MoteGlow)
        };

        public static Material[] Sparks = new[]
        {
            MaterialPool.MatFrom("Items/Weapons/Projectile/Electric/Sparks/spark1", ShaderDatabase.MoteGlow),
            MaterialPool.MatFrom("Items/Weapons/Projectile/Electric/Sparks/spark2", ShaderDatabase.MoteGlow),
            MaterialPool.MatFrom("Items/Weapons/Projectile/Electric/Sparks/spark3", ShaderDatabase.MoteGlow),
            MaterialPool.MatFrom("Items/Weapons/Projectile/Electric/Sparks/spark4", ShaderDatabase.MoteGlow)
        };
    }
}
