using Verse;

namespace TR;

public class GameComponent_TR : GameComponent
{
    //Discovery
    public ResearchDiscoveryTable ResearchDiscoveryTable;

    private static GameObject RootHolder;

    public TiberiumRoot MainRoot;

    public GameComponent_TR(Game game)
    {
        GenerateInfos();
        RootHolder = new GameObject("TiberiumRimHolder");
        Object.DontDestroyOnLoad(RootHolder);
        RootHolder.AddComponent<TiberiumRoot>();
        MainRoot = RootHolder.GetComponent<TiberiumRoot>();
    }

    private void GenerateInfos()
    {
        ResearchDiscoveryTable ??= new ResearchDiscoveryTable();
    }

    public static GameComponent_TR TRComp()
    {
        return Current.Game.GetComponent<GameComponent_TR>();
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Deep.Look(ref ResearchDiscoveryTable, "ResearchDiscoveryTable");

        if (Scribe.mode == LoadSaveMode.PostLoadInit) GenerateInfos();
    }
}