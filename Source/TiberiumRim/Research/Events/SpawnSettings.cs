using System.Collections.Generic;

namespace TR
{
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
        public List<ThingValue> spawnList = new List<ThingValue>();
        public List<SkyfallerValue> skyfallers = new List<SkyfallerValue>();
    }
}
