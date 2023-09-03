using Verse;

namespace TR
{
    public abstract class WorldInfo : IExposable
    {
        private RimWorld.Planet.World world;

        public WorldInfo(RimWorld.Planet.World world)
        {
            this.world = world;
        }

        public virtual void ExposeData()
        {
        }

        public virtual void Setup()
        {
        }

        public virtual void InfoTick()
        {
        }
    }
}
