using Verse;

namespace TiberiumRim
{
    public class MapInformation : IExposable
    {
        protected Map map;

        private bool initialized = false;

        public bool HasBeenInitialized => initialized;

        public Map Map => map;

        public MapInformation(Map map)
        {
            this.map = map;
        }

        public virtual void InfoInit(bool initAfterReload = false)
        {
            initialized = true;
        }

        public virtual void ExposeData()
        {
            Scribe_Values.Look(ref initialized, "mapInfoInit");
        }

        public virtual void Tick()
        {
        }

        public virtual void UpdateOnGUI()
        {
        }

        public virtual void Draw()
        {
        }
    }
}
