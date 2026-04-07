using System.Collections.Generic;
using TR.Hediffs.TiberiumInfection;
using TR.TiberiumEnvironment.Conversions;
using Verse;

namespace TR.TiberiumPawns;

public class AnimalMutationDef : Def
{
    public List<AnimalConversion> conversions = new();

    public TiberiumKindDef TiberiumFiendFor(PawnKindDef kind)
    {
        return conversions.FirstOrDefault(c => c.HasOutcomesFor(kind))?.toPawn;
    }
}