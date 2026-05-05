using Verse;

namespace TeleCore.Types.Structs;

public struct ProjectileLaunchedArgs
{
    public Projectile Projectile { get; }

    public ProjectileLaunchedArgs(Projectile projectile)
    {
        Projectile = projectile;
    }
}