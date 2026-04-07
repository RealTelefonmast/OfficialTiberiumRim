using Verse;

namespace TeleCore.ThingComps.Props;

public class CompProperties_GlowerOffset : CompProperties
{
    public ThingDef glowerDef;

    public CompProperties_GlowerOffset()
    {
        compClass = typeof(CompGlowerOffset);
    }
}