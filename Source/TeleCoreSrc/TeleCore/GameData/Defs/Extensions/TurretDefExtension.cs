using System.Collections.Generic;
using TeleCore.TeleTurrets.Hubs;
using Verse;

namespace TeleCore.GameData.Defs.Extensions;

public class TurretDefExtension : DefModExtension
{
    public TurretHubProperties hub;
    public List<TurretProperties>? turrets;

    public bool Invalid => turrets.NullOrEmpty();

    public bool HasTurrets => turrets is { Count: > 0 };
}