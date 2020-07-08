using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace TiberiumRim
{
    //TODO: Add ability to discover things on even trigger
    //TODO: Add new events to according tasks

    //TODO: Expand EventScannerTable
    public class DiscoveryProperties
    {
        public List<string> thingsToDiscover;

        public void Discover()
        {
            thingsToDiscover.ForEach(d => TRUtils.DiscoveryTable().Discover(d));
        }

    }

    public class EventTriggerDef : Def
    {
        public TargetProperties targets;
        public List<HediffDef> hediffs;

        public List<EventDef> eventsToTrigger;
    }


    public class EventDef : Def
    {
        [Unsaved]
        public List<TResearchDef> unlocksResearch = new List<TResearchDef>();

        public Type eventClass = typeof(BaseEvent);
        public DiscoveryProperties discoveries;
        public LetterProperties letter;

        public float activeDays = 0;

        //TODO: Implement full use of incidentproperties
        public List<IncidentProperties> incidents;

        public int ActiveTimeTicks => (int)(GenDate.TicksPerDay * activeDays);
        public bool Instant => ActiveTimeTicks == 0;
        public bool IsActive => TRUtils.EventManager().IsActive(this);
        public bool IsFinished => TRUtils.EventManager().IsFinished(this);

        public override void ResolveReferences()
        {
            foreach (var research in DefDatabase<TResearchDef>.AllDefs)
            {
                if (research.requisites?.events?.Contains(this) ?? false)
                {
                    unlocksResearch.Add(research);
                }
            }
        }
    }
}
