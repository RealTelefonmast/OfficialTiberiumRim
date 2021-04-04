using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace TiberiumRim
{
    public static class UniquePawnGenerator
    {

        public static Pawn GeneratePawn(UniquePawnDef def)
        {
            Pawn pawn = (Pawn) ThingMaker.MakeThing(def.kindDef.race);

            return null;
        }

        private static void GenerateGearFor(Pawn pawn, UniquePawnDef def)
        {
            //Apparel

            //Weapon

            //Inventory
        }
    }
}
