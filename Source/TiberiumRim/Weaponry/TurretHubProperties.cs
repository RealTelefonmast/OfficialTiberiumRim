using Verse;

namespace TR
{
    public class TurretHubProperties
    {
        public bool isHub = false;
        public ThingDef hubDef;
        public ThingDef turretDef;
        public GraphicData cableGraphic;
        public string cableTexturePath;
        public int maxTurrets = 3;
        public float connectRadius = 7.9f;
    }
}
