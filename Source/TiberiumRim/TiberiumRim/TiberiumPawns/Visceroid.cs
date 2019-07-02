using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace TiberiumRim
{
    public class Visceroid : Pawn_Visceral
    {
        public bool TryFindOther(out List<Visceroid> list)
        {
            list = new List<Visceroid>();
            return false;
        }
    }
}
