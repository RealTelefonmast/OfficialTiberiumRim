using System.Collections.Generic;
using Verse;
using RimWorld;

namespace TiberiumRim
{
    public class Gas_Tiberium : Gas
    {
        private float gasFlt = (TiberiumRimSettings.settings.InfectionTouch / 10f)/GenTicks.TickLongInterval;

        public override void Tick()
        {
            IntVec3 c = this.Position;
            if (c.InBounds(Map))
            {
                List<Thing> thinglist = c.GetThingList(Map);
                foreach (Thing t in thinglist)
                {
                    if (t is Pawn p)
                    {
                        TRUtils.Infect(p, gasFlt, Map, true);
                    }
                }
            }
            base.Tick();
        }
    }
}