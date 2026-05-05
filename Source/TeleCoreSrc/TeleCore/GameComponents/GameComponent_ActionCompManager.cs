using System.Collections.Generic;
using TeleCore.Types.Exposables;
using Verse;

namespace TeleCore.GameComponents;

public class GameComponent_ActionCompManager : GameComponent
{
    private readonly List<ActionComposition> Compositions = new();

    public GameComponent_ActionCompManager(Game game)
    {
    }

    public void InitComposition(ActionComposition composition)
    {
        Compositions.Add(composition);
    }

    public void RemoveComposition(ActionComposition composition)
    {
        Compositions.Remove(composition);
    }

    public override void GameComponentTick()
    {
        base.GameComponentTick();
        for (var i = Compositions.Count - 1; i >= 0; i--) Compositions[i].Tick();
    }
}