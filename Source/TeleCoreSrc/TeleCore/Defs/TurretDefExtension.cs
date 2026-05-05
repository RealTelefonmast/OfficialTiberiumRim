using System.Collections.Generic;
using TeleCore.Types;
using Verse;

namespace TeleCore.Defs;

public class TurretDefExtension : DefModExtension
{
    public TurretHubProperties hub;
    public List<TurretProperties>? turrets;

    public bool Invalid => turrets.NullOrEmpty();

    public bool HasTurrets => turrets is { Count: > 0 };
}