using UnityEngine;
using Verse;

namespace TR.GameParts.GameUpdate;

public class GameComponent_TR : GameComponent
{
    private static GameObject RootHolder;

    public TiberiumRoot MainRoot;

    public GameComponent_TR(Game game)
    {
        RootHolder = new GameObject("TiberiumRimHolder");
        Object.DontDestroyOnLoad(RootHolder);
        RootHolder.AddComponent<TiberiumRoot>();
        MainRoot = RootHolder.GetComponent<TiberiumRoot>();
    }

    public static GameComponent_TR TRComp()
    {
        return Current.Game.GetComponent<GameComponent_TR>();
    }

    public override void ExposeData()
    {
        base.ExposeData();
    }

    public override void FinalizeInit()
    {
        base.FinalizeInit();
    }

    public override void GameComponentTick()
    {
    }

    public override void GameComponentUpdate()
    {
        base.GameComponentUpdate();
    }

    public override void GameComponentOnGUI()
    {
        base.GameComponentOnGUI();
    }
}
