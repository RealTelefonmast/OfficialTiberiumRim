using Verse;

namespace TR.Interfaces;

public interface IResearchCraneTarget
{
    public Building ResearchCrane { get; }
    public bool ResearchBound { get; }
}