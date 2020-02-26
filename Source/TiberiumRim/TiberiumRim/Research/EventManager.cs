using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld.Planet;

namespace TiberiumRim
{
    public class EventManager : WorldComponent
    {
        public Dictionary<EventDef, bool> triggeredEvents = new Dictionary<EventDef, bool>();

        public EventManager(World world) : base(world)
        {
        }

        public void TriggerEvent(EventDef def)
        {
            triggeredEvents.Add(def, true);
            def.Worker.TryTrigger();
        }

        public bool HasBeenTriggered(EventDef def)
        {
            return triggeredEvents.TryGetValue(def, out bool value) && value;
        }
    }
}
