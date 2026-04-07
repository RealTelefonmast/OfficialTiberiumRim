using Verse;

namespace TR;

public class TiberiumCoreSettings : ModSettings
{
    private bool useCustomBG;

    public bool UseCustomBackground
    {
        get => useCustomBG;
        set
        {
            useCustomBG = value;
            Write();
        }
    }

    public static TiberiumCoreSettings Settings => TiberiumCoreMod.CoreSettings;

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref useCustomBG, "useCustomBG");
        if (Scribe.mode == LoadSaveMode.PostLoadInit)
            if (UseCustomBackground)
                LongEventHandler.QueueLongEvent(delegate { Settings.UseCustomBackground = true; }, string.Empty, false,
                    null, false);
    }
}