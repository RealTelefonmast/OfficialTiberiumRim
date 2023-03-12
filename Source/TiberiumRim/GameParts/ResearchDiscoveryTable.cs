using System.Collections.Generic;
using Verse;

namespace TiberiumRim
{
    public class ResearchDiscoveryTable : IExposable
    {
        public Dictionary<TResearchDef, bool> DiscoveredResearch = new Dictionary<TResearchDef, bool>();

        public void ExposeData()
        {
            Scribe_Collections.Look(ref DiscoveredResearch, "discoveredResearch");
        }
        
        //Research Discovery
        public bool ResearchHasBeenSeen(TResearchDef research)
        {
            return DiscoveredResearch.TryGetValue(research, out bool value) && value;
        }

        public void DiscoverResearch(TResearchDef research)
        {
            if (ResearchHasBeenSeen(research)) return;
            DiscoveredResearch.Add(research, true);
        }
    }
}
