using System.Collections.Generic;
using System.Linq;
using Verse;

namespace TiberiumRim
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
