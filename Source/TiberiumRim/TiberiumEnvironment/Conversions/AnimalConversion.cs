using System.Collections.Generic;
using Verse;

namespace TiberiumRim
{
    public class AnimalConversion
    {
        public List<PawnKindDef> fromPawn;
        public TiberiumKindDef toPawn;

        public bool HasOutcomesFor(Pawn pawn)
        {
            return HasOutcomesFor(pawn.kindDef);
        }

        public bool HasOutcomesFor(PawnKindDef kindDef)
        {
            return fromPawn.Contains(kindDef);
        }

        public PawnKindDef GetOutcomeFor(PawnKindDef def)
        {
            return HasOutcomesFor(def) ? toPawn : null;
        }
    }
}
