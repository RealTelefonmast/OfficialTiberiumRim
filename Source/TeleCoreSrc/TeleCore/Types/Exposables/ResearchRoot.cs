using TeleCore.WorldComponents;
using Verse;

namespace TeleCore.Types.Exposables;

public class ResearchRoot : IExposable
{
    private static TResearchManager? _researchManager;
    private static TEventManager? _eventManager;
    private static TResearchDiscoveryTable? _discoveryTable;

    public static TResearchManager Research => _researchManager ??= Find.World.GetComponent<TResearchManager>();
    public static TEventManager Events => _eventManager ??= new TEventManager();
    public static TResearchDiscoveryTable DiscoveryTable => _discoveryTable ??= new TResearchDiscoveryTable();

    public void ExposeData()
    {
        Scribe_Deep.Look(ref _researchManager, "researchManager");
        Scribe_Deep.Look(ref _eventManager, "eventManager");
        Scribe_Deep.Look(ref _discoveryTable, "discoveryTable");
    }
}