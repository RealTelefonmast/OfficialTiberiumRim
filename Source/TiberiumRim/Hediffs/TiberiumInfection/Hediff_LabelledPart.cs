using Verse;

namespace TiberiumRim
{
    public class Hediff_LabelledPart : HediffWithComps
    {
        public override string LabelBase => def.label.Formatted(Part.Label);
    }
}
