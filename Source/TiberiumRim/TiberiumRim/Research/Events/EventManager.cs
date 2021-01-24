using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld.Planet;
using Verse;

namespace TiberiumRim
{
    public class EventManager : WorldComponent
    {
        public List<EventDef> newEvents = new List<EventDef>();
        public List<BaseEvent> allEvents = new List<BaseEvent>();
        public Dictionary<EventDef, bool> currentEvents = new Dictionary<EventDef, bool>();

        public EventManager(World world) : base(world)
        {
            newEvents = DefDatabase<EventDef>.AllDefsListForReading;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref newEvents, "newEvents");
            Scribe_Collections.Look(ref allEvents, "allEvents");
            Scribe_Collections.Look(ref currentEvents, "currentEvents");
        }

        public override void WorldComponentTick()
        {
            base.WorldComponentTick();
            for (int i = allEvents.Count - 1; i >= 0; i--)
            {
                var curEvent = allEvents[i];
                if (!currentEvents[curEvent.def])
                {
                    curEvent.EventTick();
                }
            }
        }

        public void CheckForEventStart<T>(T trigger)
        {
            if (newEvents.NullOrEmpty()) return;
            for (var i = newEvents.Count - 1; i >= 0; i--)
            {
                var newEvent = newEvents[i];
                if (newEvent.triggerProps == null) continue;
                if (newEvent.triggerProps.TriggersEvent(trigger, out LookTargets targets))
                {
                    StartEvent(newEvent, targets);
                }
            }
        }

        public BaseEvent StartEvent(EventDef def, LookTargets targets = null)
        {
            if (def.IsActive)
            {
                Log.Warning("Trying to start event " + def.LabelCap + " which is already started!");
                return def.cachedEvent;
            }
            BaseEvent baseEvent = (BaseEvent) Activator.CreateInstance(def.eventClass);
            baseEvent.StartEvent(def);
            baseEvent.EventTargets = targets;
            def.cachedEvent = baseEvent;
            allEvents.Add(baseEvent);
            currentEvents.Add(baseEvent.def, false);

            newEvents.Remove(def);
            return baseEvent;
        }

        public void Notify_EventFinished(BaseEvent baseEvent)
        {
            currentEvents[baseEvent.def] = true;
        }

        public bool IsActive(EventDef def)
        {
            return currentEvents.TryGetValue(def, out bool value);
        }

        public bool IsFinished(EventDef def)
        {
            return currentEvents.TryGetValue(def, out bool value) && value;
        }
    }
}
