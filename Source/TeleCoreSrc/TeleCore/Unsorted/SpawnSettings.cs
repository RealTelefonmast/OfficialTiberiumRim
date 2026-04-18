using System.Collections.Generic;

namespace TeleCore.Unsorted;

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
    public List<ThingValue> spawnList = new();
    public List<SkyfallerValue> skyfallers = new();
}