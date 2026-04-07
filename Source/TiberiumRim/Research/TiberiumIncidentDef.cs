using System.Collections.Generic;
using RimWorld;
using TR.ThingData;

namespace TR.Research;

public class TiberiumIncidentDef : IncidentDef
{
    public List<EventDef> eventsToTrigger;

    //public PositionFilter positionFilter;
    public List<SkyfallerValue> skyfallers;
}