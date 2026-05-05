// Preserved from TiberiumRim/Research/Events/SpawnSettings.cs
// Note: SkyfallerValue and ThingValue are TiberiumRim-specific types.

using System.Collections.Generic;

namespace TeleCore.Research.Events;

public enum SpawnMode
{
    Stockpile,
    Target,
    DropPod,
    Scatter
}

public class SpawnSettings
{
    public SpawnMode mode = SpawnMode.Stockpile;
    public bool singleChance = false;
    // public List<SkyfallerValue> skyfallers = new();  // TiberiumRim type
    // public List<ThingValue> spawnList = new();       // TiberiumRim type
}
