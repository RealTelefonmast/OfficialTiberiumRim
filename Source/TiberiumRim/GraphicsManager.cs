using Verse;

namespace TR
{
    public class GraphicsManager : GameComponent
    {
        public GraphicsManager()
        {
        }

        public GraphicsManager(Game game)
        {
        }

        public static GraphicsManager Manager
        {
            get
            {
                return Current.Game.GetComponent<GraphicsManager>();
            }
        }

        public bool CanGlow
        {
            get
            {
                return TiberiumSettings.Settings.graphicsSettings.TiberiumGlow;
            }
        }
    }
}
