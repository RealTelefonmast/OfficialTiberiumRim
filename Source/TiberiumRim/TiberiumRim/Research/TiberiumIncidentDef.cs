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
        public List<SkyfallerProperties> skyfallers;
        public PositionFilter positions;
        public List<EventDef> eventsToTrigger;

    }
}
