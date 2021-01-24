using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace TiberiumRim
{
    public class TiberiumIncidentDef : IncidentDef
    {
        public List<EventDef> eventsToTrigger;
        //public PositionFilter positionFilter;
        public List<SkyfallerValue> skyfallers;
    }
}
