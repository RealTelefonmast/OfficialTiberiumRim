using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace TiberiumRim
{
    public static class GenTiberiumFiends
    {
        public static List<TiberiumPawn> TrySpawnFiendsNear(TiberiumBlossom blossom)
        {
            List<TiberiumPawn> pawns = new List<TiberiumPawn>();
            int count = blossom.FieldCells.Count / 5;
            for (int i = 0; i < count; i++)
            {

            }
            return pawns;
        }
    }
}
