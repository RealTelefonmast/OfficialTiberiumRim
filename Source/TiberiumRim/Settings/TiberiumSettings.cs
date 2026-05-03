using Verse;

namespace TiberiumRim;

public class TiberiumSettings : ModSettings
{
    public float BuildingDamageMltp = 1f;
    public bool CustomBackground = true;
    public bool EVASystem = true;

    //Graphics
    public GraphicsSettings graphicsSettings = new();
    public float GrowthRate = 1f;

    //Tiberium Events
    public float InfectionMltp = 1f;
    public float ItemDamageMltp = 1f;

    //PlaySettings
    public bool ShowNetworkValues;
    public float SpreadMltp = 1f;


    //Debug

    public bool startedOnce = false;

    public void SetValue<T>(ref T field, T value)
    {
        field = value;
    }

    public void SetEasy()
    {
    }

    public void SetMedium()
    {
    }

    public void SetHard()
    {
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Deep.Look(ref graphicsSettings, "graphics");
        Scribe_Deep.Look(ref ShowNetworkValues, "ShowNetworkValues");
    }
}