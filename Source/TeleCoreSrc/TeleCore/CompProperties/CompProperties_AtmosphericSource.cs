using TeleCore.Comps;
using TeleCore.Defs;

namespace TeleCore.CompProperties;

public class CompProperties_AtmosphericSource : Verse.CompProperties
{
    public int pushAmount;
    public int pushInterval;
    public AtmosphericValueDef atmosphericDef;

    public CompProperties_AtmosphericSource()
    {
        compClass = typeof(Comp_AtmosphericSource);
    }
}
