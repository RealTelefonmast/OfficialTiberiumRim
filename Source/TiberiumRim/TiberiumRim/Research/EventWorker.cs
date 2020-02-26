using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace TiberiumRim
{
    public class EventWorker
    {
        private EventDef def;

        public EventWorker(EventDef def)
        {
            this.def = def;
        }

        public void TryTrigger()
        {
            if (CanTrigger())
                TriggerAction();
        }

        public virtual void TriggerAction()
        {
        }

        public virtual bool CanTrigger()
        {
            return true;
        }
    }
}
