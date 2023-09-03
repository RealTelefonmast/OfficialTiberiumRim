using System;
using System.Collections.Generic;
using RimWorld;
using TeleCore;
using Verse;

namespace TR
{
    public class DiscoveryList
    {
        public List<DiscoveryDef> thingsToDiscover;

        public void Discover(string desc = default)
        {
            thingsToDiscover.ForEach(d => d.Discover());
        }
    }    

    public class EventTriggerProperties
    {
        public TargetProperties targets;
        public List<HediffDef> hediffs;

        public bool TriggersEvent<T>(T obj, out LookTargets lookTargets)
        {
            lookTargets = null;
            if (obj is Hediff hediff)
            {
                if (hediffs.NullOrEmpty()) return false;
                lookTargets = hediff.pawn;
                return hediffs.Contains(hediff.def);
            }
            if (obj is Thing thing)
            {
                if (targets == null) return false;
                lookTargets = thing;
                return targets.Accepts(thing);
            }
            return false;
        }
    }

    public class EventDef : Def
    {
        [Unsaved]
        public List<TResearchDef> unlocksResearch = new List<TResearchDef>();
        [Unsaved()] 
        public BaseEvent cachedEvent;

        public Type eventClass = typeof(BaseEvent);
        public EventTriggerProperties triggerProps;
        public DiscoveryList discoveries;
        public LetterProperties letter;

        public float activeDays = 0;

        //TODO: Implement full use of incidentproperties
        public List<IncidentProperties> incidents;

        public int ActiveTimeTicks => (int)(GenDate.TicksPerDay * activeDays);
        public bool Instant => ActiveTimeTicks == 0;
        public bool Activated => IsFinished || IsActive;
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
