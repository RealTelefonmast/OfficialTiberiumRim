using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace TiberiumRim
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
