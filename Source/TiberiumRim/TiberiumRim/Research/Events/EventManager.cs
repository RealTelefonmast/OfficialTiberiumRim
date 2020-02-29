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
        public Dictionary<EventDef, bool> finishedEvents = new Dictionary<EventDef, bool>();

        public EventManager(World world) : base(world)
        {
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref allEvents, "allEvents");
            Scribe_Collections.Look(ref finishedEvents, "finishedEvents");
        }

        public override void WorldComponentTick()
        {
            base.WorldComponentTick();
            for (int i = allEvents.Count - 1; i >= 0; i--)
            {
                var @event = allEvents[i];
                if (!finishedEvents[@event.def])
                {
                    @event.EventTick();
                }
            }
        }

        public void StartEvent(EventDef def)
        {
            BaseEvent baseEvent = (BaseEvent) Activator.CreateInstance(def.eventClass);
            baseEvent.StartEvent(def);
            allEvents.Add(baseEvent);
            finishedEvents.Add(baseEvent.def, false);
            
            Log.Message("Starting new event! " + def);
        }

        public void Notify_EventFinished(BaseEvent baseEvent)
        {
            finishedEvents[baseEvent.def] = true;
            Log.Message("Finishing new event! " + baseEvent.def);
        }

        public bool HasBeenFinished(EventDef def)
        {
            return finishedEvents.TryGetValue(def, out bool value) && value;
        }
    }
}
