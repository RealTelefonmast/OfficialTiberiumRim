// Preserved from TiberiumRim/Research/Events/RaidSettings.cs

using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI.Group;

namespace TeleCore.Research;

public enum RaidMode
{
    DirectAttack,
    StageThenAttack,
    Siege
}

public class RaidSettings
{
    public PawnsArrivalModeDef arriveMode;
    public bool canKidnap = true;
    public bool canLeave = true;
    public bool canSteal = false;
    public FactionDef faction;
    public RaidMode mode = RaidMode.DirectAttack;
    public bool sappers = false;
    public bool smart = false;
    public RaidStrategyDef strategy;

    public RaidSettings()
    {
        DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "faction", "Pirate");
        DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "arriveMode", "EdgeWalkIn");
        DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "strategy", "ImmediateAttack");
    }

    public void MakeLords(IncidentParms parms, Map map, List<Pawn> pawns)
    {
        var list = IncidentParmsUtility.SplitIntoGroups(pawns, parms.pawnGroups);
        foreach (var list2 in list)
        {
            LordMaker.MakeNewLord(parms.faction, MakeLordJob(parms, map, list2), map, list2);
        }
    }

    public LordJob MakeLordJob(IncidentParms parms, Map map, List<Pawn> pawns)
    {
        var originCell = !parms.spawnCenter.IsValid ? pawns[0].PositionHeld : parms.spawnCenter;
        if (parms.faction.HostileTo(Faction.OfPlayer))
        {
            if (mode == RaidMode.DirectAttack)
                return new LordJob_AssaultColony(parms.faction, canKidnap, canLeave, sappers, smart, canSteal);
            if (mode == RaidMode.StageThenAttack)
            {
                var entrySpot = !parms.spawnCenter.IsValid ? pawns[0].PositionHeld : parms.spawnCenter;
                IntVec3 stageLoc = RCellFinder.FindSiegePositionFrom_NewTemp(entrySpot, map);
                return new LordJob_StageThenAttack(parms.faction, stageLoc, Rand.Int);
            }

            if (mode == RaidMode.Siege)
            {
                var entrySpot = !parms.spawnCenter.IsValid ? pawns[0].PositionHeld : parms.spawnCenter;
                IntVec3 siegeSpot = RCellFinder.FindSiegePositionFrom_NewTemp(entrySpot, map);
                var num = parms.points * Rand.Range(0.2f, 0.3f);
                if (num < 60f) num = 60f;
                return new LordJob_Siege(parms.faction, siegeSpot, num);
            }
        }
        else
        {
            RCellFinder.TryFindRandomSpotJustOutsideColony(originCell, map, out var fallbackLocation);
            return new LordJob_AssistColony(parms.faction, fallbackLocation);
        }

        return null;
    }
}
