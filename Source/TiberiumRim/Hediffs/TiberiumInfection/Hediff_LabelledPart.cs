using Verse;

namespace TR.Hediffs.TiberiumInfection;

public class Hediff_LabeledPart : HediffWithComps
{
    public override string LabelBase => def.label.Formatted(Part.Label);
}