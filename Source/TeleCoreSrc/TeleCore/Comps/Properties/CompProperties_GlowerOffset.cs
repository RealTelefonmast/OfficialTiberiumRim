using Verse;

namespace TeleCore;

public class CompProperties_GlowerOffset : CompProperties
{
    public ThingDef glowerDef;

    public CompProperties_GlowerOffset()
    {
        compClass = typeof(CompGlowerOffset);
    }
}