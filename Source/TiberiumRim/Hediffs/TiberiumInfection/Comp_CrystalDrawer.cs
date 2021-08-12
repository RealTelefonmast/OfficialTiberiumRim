using Verse;

namespace TiberiumRim
{
    public class Comp_CrystalDrawer : ThingComp
    {
        private PawnCrystalDrawer drawer;

        public PawnCrystalDrawer Drawer
        {
            get
            {
                if (drawer == null)
                {
                    drawer = new PawnCrystalDrawer(parent as Pawn);
                }
                return drawer;
            }
        }
    }

    public class CompProperties_CrystalDrawer : CompProperties
    {
        public CompProperties_CrystalDrawer()
        {
            compClass = typeof(Comp_CrystalDrawer);
        }
    }
}
