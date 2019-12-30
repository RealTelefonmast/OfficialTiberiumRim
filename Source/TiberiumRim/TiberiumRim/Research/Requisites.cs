using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace TiberiumRim
{
    public class Requisites 
    {
        public List<ResearchProjectDef> research = new List<ResearchProjectDef>();
        public List<TResearchDef> tiberiumResearch = new List<TResearchDef>();
        public List<EventDef> events = new List<EventDef>();

        public bool Completed => ResearchComplete && TResearchComplete && EventsHappened;

        private bool ResearchComplete => research.Any() && research.All(s => s.IsFinished);
        private bool TResearchComplete => tiberiumResearch.Any() && tiberiumResearch.All(s => s.IsFinished);
        
        //TODO: Add events
        private bool EventsHappened => true;

    }
}
