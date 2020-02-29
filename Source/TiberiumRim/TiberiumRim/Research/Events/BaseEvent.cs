using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace TiberiumRim
{
    public class BaseEvent
    {
        public EventDef def;
        private int startTick = 0;
        private int endTick = 0;

        public void StartEvent(EventDef def)
        {
            this.def = def;
            this.startTick = Find.TickManager.TicksGame;
            this.endTick = startTick + def.ActiveTimeTicks;
        }

        public void FinishEvent()
        {
            TRUtils.EventManager().Notify_EventFinished(this);
        }

        public void EventTick()
        {
            int tick = Find.TickManager.TicksGame;
            if (tick > endTick)
            {
                FinishEvent();
                return;
            }

            if (CanDoEventAction(tick)) EventAction();
        }

        public virtual void EventAction()
        {

        }

        public virtual bool CanDoEventAction(int curTick)
        {
            return curTick == endTick;
        }

        public string[] DescArguments => null;

        public string TimeReadOut
        {
            get
            {
                /*
                float days = ticksLeft.TicksToDays();
                float hours = GenDate.ToStringTicksToDays() GenDate.TicksPerHour;
                float minutes = ;
                float seconds = ;
                */
                return (endTick - Find.TickManager.TicksGame).ToStringTicksToPeriodVerbose(true, false);
            }
        }
    }
}
