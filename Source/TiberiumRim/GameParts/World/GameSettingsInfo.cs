using Verse;

namespace TR
{
    public class GameSettingsInfo : WorldInfo
    {
        //PlaySettings
        public bool ShowNetworkValues = false;
        public bool EVASystem = true;
        public bool RadiationOverlay = false;

        public GameSettingsInfo(RimWorld.Planet.World world) : base(world)
        {
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref ShowNetworkValues, "ShowNetworkValues");
            Scribe_Values.Look(ref EVASystem, "EVASystem");
            Scribe_Values.Look(ref RadiationOverlay, "RadiationOverlay");
        }
    }
}
