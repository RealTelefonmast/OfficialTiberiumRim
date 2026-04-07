using TeleCore.RWExtended.Verbs.Other;

namespace TR.Weaponry.Verbs;

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
            SetProjectile(Props.defaultProjectile);
    }
}