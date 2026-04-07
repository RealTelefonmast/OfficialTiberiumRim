using TeleCore.Atmosphere.Defs;
using Verse;

namespace TeleCore.Atmosphere.Comps;

public class CompProperties_AtmosphericSource : CompProperties
{
    public int pushAmount;
    public int pushInterval;
    public AtmosphericValueDef atmosphericDef;

    public CompProperties_AtmosphericSource()
    {
        this.compClass = typeof(Comp_AtmosphericSource);
    }
}
