using TeleCore.Comps;
using Verse;

namespace TeleCore.CompProperties;

public class CompProperties_GlowerOffset : Verse.CompProperties
{
    public ThingDef glowerDef;

    public CompProperties_GlowerOffset()
    {
        compClass = typeof(CompGlowerOffset);
    }
}