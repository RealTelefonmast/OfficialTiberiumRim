using System.Collections.Generic;
using Verse;

namespace TR.Info;

public class MapPawnInfo : MapInformation
{
    public Dictionary<HediffDef, List<Pawn>> InfectedPawns = new();
    public List<Pawn> TotalSickColonists = new();
    public List<Pawn> TotalSickPawns = new();

    public MapPawnInfo(Map map) : base(map)
    {
    }

    //TODO: Pawn Registering For Alerts
    public void RegisterPawn(Pawn pawn, HediffDef def)
    {
        TotalSickPawns.Add(pawn);
        if (pawn.IsColonist)
            TotalSickColonists.Add(pawn);

        if (InfectedPawns.ContainsKey(def))
            InfectedPawns[def].Add(pawn);
        else
            InfectedPawns.Add(def, new List<Pawn> { pawn });
    }

    public void DeregisterPawns(Pawn pawn)
    {
        TotalSickPawns.Remove(pawn);
        TotalSickColonists.Remove(pawn);
        foreach (var def in InfectedPawns.Keys) InfectedPawns[def].Remove(pawn);
    }
}