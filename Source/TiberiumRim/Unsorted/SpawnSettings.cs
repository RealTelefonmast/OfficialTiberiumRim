using System.Collections.Generic;

namespace TiberiumRim;

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
    public List<SkyfallerValue> skyfallers = new();
    public List<ThingValue> spawnList = new();
}