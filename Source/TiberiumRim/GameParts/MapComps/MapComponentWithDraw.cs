using Verse;

namespace TR
{
    public abstract class MapComponentWithDraw : MapComponent
    {
        public MapComponentWithDraw(Map map) : base(map) { }

        //TODO: Draw Method Update call needs patch fix to transpiler
        //Draw Update Call on low-level for optimized execution
        public virtual void MapComponentDraw()
        {
        }
    }
}
