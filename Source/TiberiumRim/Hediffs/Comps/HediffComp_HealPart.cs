using Verse;

namespace TR.Comps;

public class HediffComp_HealPart : HediffComp
{
}

public class HediffCompProperties_HealPart : HediffCompProperties
{
    public float healRate = 1;

    public HediffCompProperties_HealPart()
    {
        compClass = typeof(HediffComp_HealPart);
    }
}