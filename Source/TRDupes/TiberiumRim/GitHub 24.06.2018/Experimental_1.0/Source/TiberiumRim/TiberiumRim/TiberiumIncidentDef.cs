using RimWorld;
using Verse;

namespace TiberiumRim
{
    public class TiberiumIncidentDef : IncidentDef
    {
        public ThingDef asteroidType;

        public ThingDef skyfallerDef;

        public TiberiumCrystalDef tiberiumType;

        public bool appears = false;

        public string sourceThing;

        public bool spawnCrystals = false;
    }
}