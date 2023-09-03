using Verse;

namespace TR
{
    public class HediffComp_HealPart : HediffComp
    {

    }

    public class HediffCompProperties_HealPart : HediffCompProperties
    {
        public HediffCompProperties_HealPart()
        {
            compClass = typeof(HediffComp_HealPart);
        }

        public float healRate = 1;
    }
}
