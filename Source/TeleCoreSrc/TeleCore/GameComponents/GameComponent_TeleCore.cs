using TeleCore.Unsorted;
using Verse;

namespace TeleCore.GameComponents;

public class GameComponent_TeleCore : GameComponent
{
    private ActionCompositionHandler actionCompositionHandler;
    private readonly TeleUpdateManager updateManager;

    public GameComponent_TeleCore(Game game)
    {
        StaticData.Notify_ClearData();
        actionCompositionHandler = new ActionCompositionHandler();
        updateManager = new TeleUpdateManager();
    }

    public static GameComponent_TeleCore Instance()
    {
        return Current.Game.GetComponent<GameComponent_TeleCore>();
    }

    public override void ExposeData()
    {
        base.ExposeData();

        Scribe_Deep.Look(ref actionCompositionHandler, nameof(actionCompositionHandler));
        //Scribe_Deep.Look(ref updateManager, nameof(updateManager));
    }

    public override void FinalizeInit()
    {
        base.FinalizeInit();
    }

    public override void GameComponentTick()
    {
        actionCompositionHandler.TickActionComps();
        updateManager.Tick();
    }

    public override void GameComponentUpdate()
    {
        base.GameComponentUpdate();
        updateManager.Update();
    }

    public override void GameComponentOnGUI()
    {
        base.GameComponentOnGUI();
        updateManager.OnGUI();
    }
}