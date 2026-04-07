using TR.Util;
using Verse;

namespace TR;

public class GraphicsManager : GameComponent
{
    public GraphicsManager()
    {
    }

    public GraphicsManager(Game game)
    {
    }

    public static GraphicsManager Manager => Current.Game.GetComponent<GraphicsManager>();

    public bool CanGlow => TRUtils.TiberiumSettings().graphicsSettings.TiberiumGlow;
}