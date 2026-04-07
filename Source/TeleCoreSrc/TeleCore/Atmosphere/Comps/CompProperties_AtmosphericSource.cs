using TeleCore.Atmosphere.Defs;
using Verse;

namespace TeleCore.Atmosphere.Comps;

public class CompProperties_AtmosphericSource : CompProperties
{
    public AtmosphericValueDef atmosphericDef;
    public int pushAmount;
    public int pushInterval;

    public CompProperties_AtmosphericSource()
    {
        compClass = typeof(Comp_AtmosphericSource);
    }
}