using Verse;


namespace TiberiumRim
{
    public class TiberiumSettings : ModSettings
    {
        public bool CustomBackground = true;

        //Tiberium Events
        public float InfectionMltp = 1f;
        public float BuildingDamageMltp = 1f;
        public float ItemDamageMltp = 1f;
        public float GrowthRate = 1f;
        public float SpreadMltp = 1f;

        //Graphics
        public GraphicsSettings graphicsSettings = new GraphicsSettings();

        //PlaySettings
        public bool ShowNetworkValues = false;
        public bool EVASystem = true;

        
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
}
