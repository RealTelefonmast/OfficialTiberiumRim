using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TeleCore.Defs;
using TeleCore.Interfaces;
using TeleCore.Static;
using Verse;

namespace TeleCore.World.WorldInfo;

public class DiscoveryTable : IExposable
{
    private Dictionary<DiscoveryDef, bool> discoveries = new();

    //TODO: Research For ALL!
    //public Dictionary<TResearchDef, bool> DiscoveredResearch = new Dictionary<TResearchDef, bool>();

    public bool this[DiscoveryDef discovery] => IsDiscovered(discovery);
    public bool this[IDiscoverable discovery] => this[discovery.DiscoveryDef];

    public Dictionary<DiscoveryDef, bool> Discoveries => discoveries;

    public void ExposeData()
    {
        Scribe_Collections.Look(ref discoveries, "discoveredDict", LookMode.Def, LookMode.Value);
    }

    //Parent Discovery
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsDiscovered(DiscoveryDef discovery)
    {
        return Discoveries.TryGetValue(discovery, out var value) && value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsDiscovered(IDiscoverable discoverable)
    {
        return IsDiscovered(discoverable.DiscoveryDef);
    }

    public void Discover(DiscoveryDef discovery)
    {
        if (IsDiscovered(discovery)) return;
        Discoveries.Add(discovery, true);
        Find.LetterStack.ReceiveLetter(Translations.Discovery.DiscoveryNew,
            Translations.Discovery.DiscoveryDesc(discovery.description), TeleDefOf.DiscoveryLetter);
    }

    // //Research Discovery
    // public bool ResearchHasBeenSeen(TResearchDef research)
    // {
    //     return DiscoveredResearch.TryGetValue(research, out bool value) && value;
    // }
    //
    // public void DiscoverResearch(TResearchDef research)
    // {
    //     if (ResearchHasBeenSeen(research)) return;
    //     DiscoveredResearch.Add(research, true);
    // }
}