// Preserved from TiberiumRim/Research/Events/IncidentParmsProperties.cs

using System.Collections.Generic;
using RimWorld;
using Verse;

namespace TeleCore.Research.OldRef;

public class IncidentParmsProperties
{
    public float biocodeWeaponsChance;
    public bool dontUseSingleUseRocketLaunchers;
    public bool generateFightersOnly;
    public int pawnCount;
    public int? pawnGroupMakerSeed;
    public Dictionary<Pawn, int> pawnGroups;
    public PawnKindDef pawnKind;
    public int podOpenDelay = 140;
    public PawnsArrivalModeDef raidArrivalMode;
    public bool raidArrivalModeForQuickMilitaryAid;
    public bool raidForceOneIncap;
    public bool raidNeverFleeIndividual;

    public RaidStrategyDef raidStrategy;

    public TraderKindDef traderKind;
}
