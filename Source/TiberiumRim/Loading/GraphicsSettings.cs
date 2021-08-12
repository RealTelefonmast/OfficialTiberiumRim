using Verse;

namespace TiberiumRim
{
    public class GraphicsSettings : IExposable
    {
        public bool TiberiumFog = true;
        public bool TiberiumGlow = true;
        public bool TiberiumMulti = true;

        public void ExposeData()
        {
            Scribe_Values.Look(ref TiberiumFog, "Fog");
            Scribe_Values.Look(ref TiberiumGlow, "Glow");
            Scribe_Values.Look(ref TiberiumMulti, "Multi");
        }
    }
}
