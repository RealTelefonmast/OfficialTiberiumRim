using Verse;

namespace TiberiumRim;

public class TiberiumCrystalProperties
{
    public IntRange buildingDamage = new(0, 5);
    public bool canBeInhibited = true;
    public TiberiumConsistence consistence = TiberiumConsistence.Plantlike;
    public bool dependsOnProducer = false;
    public IntRange entityDamage = new(0, 5);
    public float growDays = 1f;
    public float harvestTime = 10f;
    public float harvestValue = 0f;
    public bool infects = true;

    public int MeshCount = 1;
    public float minTemperature = -30f;
    public float plantMutationChance = 0.5f;
    public bool radiates = true;
    public float reproduceDays = 1f;
    public FloatRange sizeRange = new(1f, 1f);
    public bool smoothSpread = false;

    public float spreadRadius = 1f;

    //These are the main properties for Tiberium Crystals
    public TiberiumValueType type = TiberiumValueType.None;
}