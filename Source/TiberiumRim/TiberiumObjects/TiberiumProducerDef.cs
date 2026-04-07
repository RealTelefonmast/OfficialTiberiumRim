using TR.Defs;
using TR.GameParts;
using Verse;

namespace TR.TiberiumObjects;

public class TiberiumProducerDef : TRThingDef
{
    public bool canBeGroundZero = false;
    public float daysToMature = 0f;
    public bool forResearch = true;
    public bool growsBlossomTree = false;
    public bool leaveTiberium = true;
    public bool mutatesArea = true;
    public SpawnProperties spawner;

    //public List<PotentialEvolution> evolutions;
    public SporeProperties spore;

    [Unsaved] private float? spreadRange;

    //Tiberium Properties
    public TiberiumFieldRuleset tiberiumFieldRules;

    public float SpreadRange => spreadRange ??= spawner?.spreadRange.RandomInRange ?? -1;
}

public class SpawnProperties
{
    public IntRange explosionRange = new(10, 100);
    public float growRadius = 5f;
    public float minProgressToSpread = 0.65f;
    public IntRange spawnInterval = new(2500, 5000);
    public TiberiumSpawnMode spawnMode = TiberiumSpawnMode.Direct;
    public float sporeExplosionRadius = 20f;
    public IntVec3 sporeOffset = new(0, 0, 0);
    public FloatRange spreadRange = new(-1, -1);
}

public class PlantGroupProperties
{
    public int minFieldSize = 1000;
    public IntRange sizeRange = new(5, 10);
}