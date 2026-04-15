using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace TiberiumRim
{
    public class JobInfo : IExposable
    {
        public object objectA;

        public object objectB;

        public object objectC;

        public Def defA;

        public Def defB;

        public Def defC;

        public JobInfo(object o1 = null, object o2 = null, object o3 = null, Def d1 = null, Def d2 = null, Def d3 = null)
        {
            objectA = o1;
            objectB = o2;
            objectC = o3;
            defA = d1;
            defB = d2;
            defC = d3;
        }

        public void ExposeData()
        {
            Scribe_Deep.Look(ref objectA, "objectA");
            Scribe_Deep.Look(ref objectB, "objectB");
            Scribe_Deep.Look(ref objectC, "objectC");
            Scribe_Defs.Look(ref defA, "defA");
            Scribe_Defs.Look(ref defB, "defB");
            Scribe_Defs.Look(ref defC, "defC");
        }
    }
}
