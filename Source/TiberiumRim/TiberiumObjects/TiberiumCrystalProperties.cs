using Verse;

namespace TR;

public class TiberiumCrystalProperties
{
    public bool canBeInhibited = true;
    public TiberiumConsistence consistence = TiberiumConsistence.Plantlike;
    public bool dependsOnProducer = false;
    public IntRange deteriorationDamage = new(0, 0);
    public float growDays = 1f;
    public float harvestValue = 0f;
    public bool infects = true;
    public int MeshCount = 1;
    public float minTemperature = -30f;
    public bool needsParent = true;
    public float plantMutationChance = 0.5f;
    public bool radiates = true;
    public float reproduceDays = 1f;
    public float rootNodeChance = 0.06f;

    public FloatRange sizeRange = new(1f, 1f);


    public float spreadRadius = 1f;

    //These are the main properties for Tiberium Crystals
    public TiberiumValueType type = TiberiumValueType.None;
}