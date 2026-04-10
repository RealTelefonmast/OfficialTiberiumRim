using TeleCore.Verbs.Other;

namespace TR.Verbs;

public class Verb_TR : Verb_ProjectileExtended
{
    public void SwitchProjectile()
    {
        if (Projectile == Props.defaultProjectile)
        {
            SetProjectile(Props.secondaryProjectile);
            return;
        }

        if (Projectile == Props.secondaryProjectile) SetProjectile(Props.defaultProjectile);
    }
}