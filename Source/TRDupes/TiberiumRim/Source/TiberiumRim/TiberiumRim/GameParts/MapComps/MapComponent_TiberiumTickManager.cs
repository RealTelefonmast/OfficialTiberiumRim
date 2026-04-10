using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace TiberiumRim
{
    public class MapComponent_TiberiumTickManager : MapComponent
    {
        public MapComponent_TiberiumTickManager(Map map) : base(map)
        {
        }

        public override void MapComponentTick()
        {
            base.MapComponentTick();
        }

        private IEnumerator TickAll()
        {
            if(Find.TickManager.Paused)
            yield return null;
        }
    }
}
