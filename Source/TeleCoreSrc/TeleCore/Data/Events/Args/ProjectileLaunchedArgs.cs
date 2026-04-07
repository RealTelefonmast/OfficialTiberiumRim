using Verse;

namespace TeleCore.Events;

public struct ProjectileLaunchedArgs
{
    public Projectile Projectile { get; }

    public ProjectileLaunchedArgs(Projectile projectile)
    {
        Projectile = projectile;
    }
}