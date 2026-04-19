using System.Collections.Generic;
using TiberiumRim;
using TR.Conversions;
using Verse;

namespace TR;

public class AnimalMutationDef : Def
{
    public List<AnimalConversion> conversions = new();

    public TiberiumKindDef TiberiumFiendFor(PawnKindDef kind)
    {
        return conversions.FirstOrDefault(c => c.HasOutcomesFor(kind))?.toPawn;
    }
}