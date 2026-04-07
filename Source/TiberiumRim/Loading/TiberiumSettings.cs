using Verse;

namespace TR.Loading;

public class TiberiumSettings : ModSettings
{
    //Tiberium Settings:
    public bool BuildingDamage = true;
    public float BuildingDamageMltp = 1f;
    public bool CustomBackground = true;
    public bool EntityDamage = true;

    //Debug Settings
    public bool FirstStartUp = true;


    //Graphics Settings
    public GraphicsSettings graphicsSettings = new();
    public float GrowthRate = 1f;

    //Specialized Settings
    public float InfectionMltp = 1f;
    public float ItemDamageMltp = 1f;
    public bool PawnDamage = true;
    public float SpreadMltp = 1f;
    public int TiberiumProducersAmt = 7;
    public bool UseCustomBackground = true;
    public bool UseProducerCap;
    public bool UseSpecificProducers;


    public bool UseSpreadRadius;

    public bool WorldSpread = true;

    public void SetValue<T>(ref T field, T value)
    {
        field = value;
    }

    public void SetEasy()
    {
        BuildingDamage = false;
        EntityDamage = false;
        PawnDamage = true;
        UseProducerCap = true;
        UseSpecificProducers = false;
        UseSpreadRadius = true;
        UseCustomBackground = true;
        TiberiumProducersAmt = 5;
        WorldSpread = true;

        InfectionMltp = 0.01f;
        BuildingDamageMltp = 0.1f;
        ItemDamageMltp = 0.1f;
        GrowthRate = 0.5f;
        SpreadMltp = 0.25f;
    }

    public void ResetToDefault()
    {
        BuildingDamage = true;
        EntityDamage = true;
        PawnDamage = true;
        UseProducerCap = false;
        UseSpecificProducers = false;
        UseSpreadRadius = false;
        UseCustomBackground = true;
        TiberiumProducersAmt = 7;
        WorldSpread = true;

        InfectionMltp = 1f;
        BuildingDamageMltp = 1f;
        ItemDamageMltp = 1f;
        GrowthRate = 1f;
        SpreadMltp = 1f;
    }

    public void SetHard()
    {
        BuildingDamage = true;
        EntityDamage = true;
        PawnDamage = true;
        UseProducerCap = false;
        UseSpecificProducers = false;
        UseSpreadRadius = false;
        UseCustomBackground = true;
        TiberiumProducersAmt = 5;
        WorldSpread = true;

        InfectionMltp = 0.45f;
        BuildingDamageMltp = 4.5f;
        ItemDamageMltp = 4f;
        GrowthRate = 2f;
        SpreadMltp = 2.5f;
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Deep.Look(ref graphicsSettings, "graphics");
        Scribe_Values.Look(ref FirstStartUp, "firstStart");
    }
}
