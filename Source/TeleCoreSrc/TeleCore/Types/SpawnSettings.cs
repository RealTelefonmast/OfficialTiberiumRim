using System.Collections.Generic;
using TeleCore.UI;

namespace TeleCore.Types;

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