using TeleCore;

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
