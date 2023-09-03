using System.Collections.Generic;
using RimWorld;
using Verse;

namespace TR
{
    public class IncidentParmsProperties
    {
        public bool generateFightersOnly;
        public bool dontUseSingleUseRocketLaunchers;

        public RaidStrategyDef raidStrategy;
        public PawnsArrivalModeDef raidArrivalMode;
        public bool raidForceOneIncap;
        public bool raidNeverFleeIndividual;
        public bool raidArrivalModeForQuickMilitaryAid;
        public float biocodeWeaponsChance;
        public Dictionary<Pawn, int> pawnGroups;
        public int? pawnGroupMakerSeed;
        public PawnKindDef pawnKind;
        public int pawnCount;

        public TraderKindDef traderKind;
        public int podOpenDelay = 140;
    }
}
