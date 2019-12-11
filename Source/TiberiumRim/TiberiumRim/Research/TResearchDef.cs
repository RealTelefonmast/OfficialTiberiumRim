using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace TiberiumRim
{
    public class TResearchDef : Def
    {
        public Requisites requisites;

        public List<EventDef> events;
    }
}
