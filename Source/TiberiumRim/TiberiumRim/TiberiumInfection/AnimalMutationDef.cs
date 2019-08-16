using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace TiberiumRim
{
    public class AnimalMutationDef : Def
    {
        public List<AnimalMutation> Mutations = new List<AnimalMutation>();

        public TiberiumKindDef TiberiumFiendFor(PawnKindDef kind)
        {
            return (TiberiumKindDef)Mutations.Find(m => m.pawnKinds.Contains(kind)).turnsInto;
        }
    }

    public class AnimalMutation
    {
        public List<PawnKindDef> pawnKinds;
        public PawnKindDef turnsInto;
    }
}
