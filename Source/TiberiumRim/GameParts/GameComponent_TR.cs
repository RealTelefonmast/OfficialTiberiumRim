using TR.Sound;
using Verse;

namespace TR.GameParts;

public class GameComponent_TR : GameComponent
{
    public SampleManager soundManager = new();

    public GameComponent_TR(Game game)
    {
    }

    public static GameComponent_TR TRComp()
    {
        return Current.Game.GetComponent<GameComponent_TR>();
    }

    public override void GameComponentUpdate()
    {
        base.GameComponentUpdate();
        soundManager.Update();
    }
}