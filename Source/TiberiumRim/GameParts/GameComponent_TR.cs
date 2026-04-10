using Verse;

namespace TR;

public class GameComponent_TR : GameComponent
{
    //Discovery
    public ResearchDiscoveryTable ResearchDiscoveryTable;

    public GameComponent_TR(Game game)
    {
        GenerateInfos();
    }

    private void GenerateInfos()
    {
        ResearchDiscoveryTable ??= new ResearchDiscoveryTable();
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Deep.Look(ref ResearchDiscoveryTable, "ResearchDiscoveryTable");

        if (Scribe.mode == LoadSaveMode.PostLoadInit) GenerateInfos();
    }
}