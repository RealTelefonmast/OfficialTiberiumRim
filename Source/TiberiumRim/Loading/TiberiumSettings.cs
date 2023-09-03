using Verse;

namespace TR;

public class TiberiumSettings : ModSettings
{
    //Tiberium Settings:
    public bool BuildingDamage = true;
    public bool EntityDamage = true;
    public bool PawnDamage = true;
    public bool UseProducerCap = false;
    public bool UseSpecificProducers = false;
    private bool useCustomBG;
    private bool firstStartUp;

    public bool UseSpreadRadius = false;
    public int TiberiumProducersAmt = 7;

    public bool WorldSpread = true;

    //Specialized Settings
    public float InfectionMltp = 1f;
    public float BuildingDamageMltp = 1f;
    public float ItemDamageMltp = 1f;
    public float GrowthRate = 1f;
    public float SpreadMltp = 1f;


    //Overlays Settings
    public GraphicsSettings graphicsSettings = new GraphicsSettings();

    //World Init Data
    public float tiberiumCoverage = 0f;

    //Special Settings
    public bool UseCustomBackground
    {
        get => useCustomBG;
        set
        {
            useCustomBG = value;
            Write();
        }
    }

    public bool FirstStartUp
    {
        get => firstStartUp;
        set
        {
            firstStartUp = value;
            Write();
        }
    }

    public static TiberiumSettings Settings => TiberiumRimMod.Settings;

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
        Write();
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
        Write();
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
        Write();
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref useCustomBG, "useCustomBG");
        Scribe_Values.Look(ref firstStartUp, "firstStart");
        Scribe_Deep.Look(ref graphicsSettings, "graphics");

        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            if (UseCustomBackground)
            {
                LongEventHandler.QueueLongEvent(delegate
                {
                    TiberiumSettings.Settings.UseCustomBackground = true;
                },  string.Empty, false, null, false);
            }
        }
    }
}