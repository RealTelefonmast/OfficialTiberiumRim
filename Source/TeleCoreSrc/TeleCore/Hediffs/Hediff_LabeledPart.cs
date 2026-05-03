using Verse;

namespace TeleCore.Unsorted;

public class Hediff_LabeledPart : HediffWithComps
{
    public override string LabelBase => def.label.Formatted(Part.Label);
}