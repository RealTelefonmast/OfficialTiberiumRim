using Verse;

namespace TR
{
    public class Hediff_LabelledPart : HediffWithComps
    {
        public override string LabelBase => def.label.Formatted(Part.Label);
    }
}
