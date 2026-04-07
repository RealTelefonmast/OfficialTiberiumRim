using Verse;

namespace TeleCore.Events.Args;

public struct ProjectileLaunchedArgs
{
    public Projectile Projectile { get; }

    public ProjectileLaunchedArgs(Projectile projectile)
    {
        Projectile = projectile;
    }
}
