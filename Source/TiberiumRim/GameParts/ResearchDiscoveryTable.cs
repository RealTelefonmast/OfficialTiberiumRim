using System.Collections.Generic;
using TeleCore.Research.Defs;
using Verse;

namespace TR;

public class ResearchDiscoveryTable : IExposable
{
    public Dictionary<TResearchDef, bool> DiscoveredResearch = new();

    public void ExposeData()
    {
        Scribe_Collections.Look(ref DiscoveredResearch, "discoveredResearch");
    }

    //Research Discovery
    public bool ResearchHasBeenSeen(TResearchDef research)
    {
        return DiscoveredResearch.TryGetValue(research, out var value) && value;
    }

    public void DiscoverResearch(TResearchDef research)
    {
        if (ResearchHasBeenSeen(research)) return;
        DiscoveredResearch.Add(research, true);
    }
}