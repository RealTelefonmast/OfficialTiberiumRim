using System.Collections.Generic;
using Verse;

namespace TR
{
    public class AnimalMutationDef : Def
    {
        public List<AnimalConversion> conversions = new List<AnimalConversion>();

        public TiberiumKindDef TiberiumFiendFor(PawnKindDef kind)
        {
            return conversions.FirstOrDefault(c => c.HasOutcomesFor(kind))?.toPawn;
        }
    }
}
