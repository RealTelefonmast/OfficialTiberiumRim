using TeleCore.Comps;
using TeleCore.Defs;

namespace TeleCore.Atmospheres.CompProperties;

public class CompProperties_AtmosphericSource : Verse.CompProperties
{
    public AtmosphericValueDef atmosphericDef;
    public int pushAmount;
    public int pushInterval;

    public CompProperties_AtmosphericSource()
    {
        compClass = typeof(Comp_AtmosphericSource);
    }
}