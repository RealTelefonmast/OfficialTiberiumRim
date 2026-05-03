using System.Collections.Generic;
using System.Linq;
using Verse;

namespace TiberiumRim;

public class TiberiumProducerDef : TRThingDef
{
    public List<TerrainSupport> customTerrain;
    public float daysToMature = 0f;
    public List<PotentialEvolution> evolutions;
    public bool forResearch = true;
    public bool growsFlora = true;
    public ThingDef killedVersion;
    public bool leaveTiberium = true;
    public List<PlantGroupChance> plantsByDistance;
    public SpawnProperties spawner = new();
    public SporeProperties spore;
    public List<TiberiumTerrainDef> tiberiumTerrain = new();
    public List<TiberiumCrystalDef> tiberiumTypes = new();

    public ThingDef SelectPlantByDistance(float distance, float maxDistance, TiberiumTerrainDef terrain)
    {
        var list = new List<ThingDef>();
        return plantsByDistance.Where(p => distance >= maxDistance * p.chance).SelectMany(p => p.plants)
            .Where(p => terrain.SupportsPlant(p.thing)).InRandomOrder().RandomElementByWeight(p => p.weight).thing;
    }
}

public class SpawnProperties
{
    public IntRange explosionRange = new(10, 100);
    public float growRadius = 5f;
    public float minDaysToSpread = 0f;
    public IntRange spawnInterval = new(2500, 5000);
    public TiberiumSpawnMode spawnMode = TiberiumSpawnMode.Direct;
    public float sporeExplosionRadius = 20f;
    public IntVec3 sporeOffset = new(0, 0, 0);
    public FloatRange spreadRange = new(-1, -1);
}

public class PlantGroupProperties
{
    public int minFieldSize = 1000;

    public List<PlantChance> plants = new();
    public IntRange sizeRange = new(5, 10);
}