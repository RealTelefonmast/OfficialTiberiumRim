using Verse;

namespace TR
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
