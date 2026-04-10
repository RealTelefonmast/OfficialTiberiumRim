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

        public virtual void MapComponentDraw()
        {
        }
    }
}
