using Verse;

namespace TR.GameParts.Interfaces;

public interface IResearchCraneTarget
{
    public Building ResearchCrane { get; }
    public bool ResearchBound { get; }
}