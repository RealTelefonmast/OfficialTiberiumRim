using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeleCore;
using Verse;

namespace TiberiumRim
{
    public class Verb_TR : Verb_ProjectileExtended
    {
        public void SwitchProjectile()
        {
            if (Projectile == Props.defaultProjectile)
            {
                SetProjectile(Props.secondaryProjectile);
                return;
            }
            if (Projectile == Props.secondaryProjectile)
            {
                SetProjectile(Props.defaultProjectile);
                return;
            }
        }
    }
}
