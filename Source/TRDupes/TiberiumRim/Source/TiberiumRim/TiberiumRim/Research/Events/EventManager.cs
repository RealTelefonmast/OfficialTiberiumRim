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
        public List<BaseEvent> allEvents = new List<BaseEvent>();
        public Dictionary<EventDef, bool> currentEvents = new Dictionary<EventDef, bool>();

        public EventManager(World world) : base(world)
        {
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref allEvents, "allEvents");
            Scribe_Collections.Look(ref currentEvents, "currentEvents");
        }

        public override void WorldComponentTick()
        {
            base.WorldComponentTick();
            for (int i = allEvents.Count - 1; i >= 0; i--)
            {
                var @event = allEvents[i];
                if (!currentEvents[@event.def])
                {
                    @event.EventTick();
                }
            }
        }

        //TODO: Implement event scanner
        //TODO: Trigger Events when certain things spawn/happen

        public void StartEvent(EventDef def)
        {
            BaseEvent baseEvent = (BaseEvent) Activator.CreateInstance(def.eventClass);
            baseEvent.StartEvent(def);
            allEvents.Add(baseEvent);
            currentEvents.Add(baseEvent.def, false);
        }

        public void Notify_EventFinished(BaseEvent baseEvent)
        {
            currentEvents[baseEvent.def] = true;
        }

        public bool IsActive(EventDef def)
        {
            return currentEvents.TryGetValue(def, out bool value) && !value;
        }

        public bool IsFinished(EventDef def)
        {
            return currentEvents.TryGetValue(def, out bool value) && value;
        }
    }
}
