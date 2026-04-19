using Verse;

namespace TR.WorldInfos;

public class TRGameSettingsInfo : WorldInformation
{
    public bool EVASystem = true;

    public bool RadiationOverlay;

    //PlaySettings
    public bool ShowNetworkValues;

    public TRGameSettingsInfo(RimWorld.Planet.World world) : base(world)
    {
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref ShowNetworkValues, "ShowNetworkValues");
        Scribe_Values.Look(ref RadiationOverlay, "RadiationOverlay");
        Scribe_Values.Look(ref EVASystem, "EVASystem");
    }
}