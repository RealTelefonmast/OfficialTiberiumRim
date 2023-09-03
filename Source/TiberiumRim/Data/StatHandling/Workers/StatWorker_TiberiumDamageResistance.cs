using RimWorld;
using Verse;

namespace TR;

public class StatWorker_TiberiumDamageResistance : StatWorker_Tiberium
{
    public override bool ShouldShowFor(StatRequest req)
    {
        return req.HasThing && req.Thing is not Pawn;
    }
}