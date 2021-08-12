using Verse;

namespace TiberiumRim
{
    public interface IResearchCraneTarget
    {
        public Building ResearchCrane { get; }
        public bool ResearchBound { get; }
    }
}
