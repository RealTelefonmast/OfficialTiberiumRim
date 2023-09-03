using System.Collections.Generic;
using Verse;

namespace TR
{
    public class EVATime
    {
        public EVASignal signal;
        public int ticks = 500;
    }

    public class EVASettingsDef : Def
    {
        public List<EVATime> times;

        public int TimeFor(EVASignal signal)
        {
            return times.Find(t => t.signal == signal).ticks;
        }
    }
}
