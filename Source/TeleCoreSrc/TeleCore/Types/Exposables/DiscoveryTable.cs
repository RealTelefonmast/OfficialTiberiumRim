using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TeleCore.Defs;
using Verse;

namespace TeleCore.Unsorted;

public class DiscoveryTable : IExposable
{
    private Dictionary<TRThingDef, bool> discoveredMenuOptions = new();
    private Dictionary<TResearchDef, bool> discoveredResearch = new();
    private Dictionary<DiscoveryDef, bool> discoveries = new();

    //TODO: Research For ALL!
    //public Dictionary<TResearchDef, bool> DiscoveredResearch = new Dictionary<TResearchDef, bool>();

    public bool this[DiscoveryDef discovery] => IsDiscovered(discovery);
    public bool this[IDiscoverable discovery] => this[discovery.DiscoveryDef];

    public Dictionary<DiscoveryDef, bool> Discoveries => discoveries;
    public Dictionary<TRThingDef, bool> DiscoveredMenuOptions => discoveredMenuOptions;
    public Dictionary<TResearchDef, bool> DiscoveredResearch => discoveredResearch;

    public void ExposeData()
    {
        Scribe_Collections.Look(ref discoveries, "discoveredDict", LookMode.Def, LookMode.Value);
        Scribe_Collections.Look(ref discoveredResearch, "discoveredResearch", LookMode.Def, LookMode.Value);
        Scribe_Collections.Look(ref discoveredMenuOptions, "menuDiscovered", LookMode.Def, LookMode.Value);
    }

    //Build Menu
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool MenuOptionHasBeenSeen(TRThingDef def)
    {
        return discoveredMenuOptions.TryGetValue(def, out var value) && value;
    }

    public void DiscoverInMenu(TRThingDef def)
    {
        if (MenuOptionHasBeenSeen(def)) return;
        discoveredMenuOptions.Add(def, true);
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

    //Research Discovery
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ResearchHasBeenSeen(TResearchDef research)
    {
        return discoveredResearch.TryGetValue(research, out var value) && value;
    }

    public void DiscoverResearch(TResearchDef research)
    {
        if (ResearchHasBeenSeen(research)) return;
        discoveredResearch.Add(research, true);
    }
}

// OLD REF (original TiberiumRim version — public fields, no LookMode params, no inlining, no indexer):
// public class DiscoveryTable : IExposable
// {
//     public Dictionary<TRThingDef, bool> DiscoveredMenuOptions = new();
//     public Dictionary<TResearchDef, bool> DiscoveredResearch = new();
//     public Dictionary<DiscoveryDef, bool> Discoveries = new();
//
//     public void ExposeData()
//     {
//         Scribe_Collections.Look(ref Discoveries, "discoveredDict");
//         Scribe_Collections.Look(ref DiscoveredResearch, "discoveredResearch");
//         Scribe_Collections.Look(ref DiscoveredMenuOptions, "menuDiscovered");
//     }
//     ...
// }
//
// OLD REF (TeleCore version — no menu/research tracking, uses TeleDefOf/Translations):
// public class DiscoveryTable : IExposable
// {
//     private Dictionary<DiscoveryDef, bool> discoveries = new();
//     public bool this[DiscoveryDef discovery] => IsDiscovered(discovery);
//     public bool this[IDiscoverable discovery] => this[discovery.DiscoveryDef];
//     ...
//     public void Discover(DiscoveryDef discovery)
//     {
//         if (IsDiscovered(discovery)) return;
//         Discoveries.Add(discovery, true);
//         Find.LetterStack.ReceiveLetter(Translations.Discovery.DiscoveryNew,
//             Translations.Discovery.DiscoveryDesc(discovery.description), TeleDefOf.DiscoveryLetter);
//     }
// }