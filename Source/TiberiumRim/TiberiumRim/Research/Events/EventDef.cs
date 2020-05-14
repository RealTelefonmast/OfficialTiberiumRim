using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace TiberiumRim
{
    public class DiscoveryProperties
    {
        public TargetProperties targets;
        public List<HediffDef> hediffs;
    }

    public class EventDef : Def
    {
        [Unsaved]
        public List<TResearchDef> unlocksResearch = new List<TResearchDef>();

        public Type eventClass = typeof(BaseEvent);
        public float activeDays = 0;
        public DiscoveryProperties discovery;

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
