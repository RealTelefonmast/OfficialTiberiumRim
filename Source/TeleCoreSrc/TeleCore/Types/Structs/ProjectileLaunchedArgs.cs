using Verse;

namespace TeleCore.Unsorted;

public struct ProjectileLaunchedArgs
{
    public Projectile Projectile { get; }

    public ProjectileLaunchedArgs(Projectile projectile)
    {
        Projectile = projectile;
    }
}