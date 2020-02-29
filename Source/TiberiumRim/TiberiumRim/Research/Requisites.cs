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

        private bool ResearchComplete => research.NullOrEmpty() || research.All(s => s.IsFinished);
        private bool TResearchComplete => tiberiumResearch.NullOrEmpty() || tiberiumResearch.All(s => s.IsFinished);
        
        //TODO: Add events
        private bool EventsHappened => events.All(e => e.HasBeenFinished);

        public override string ToString()
        {
            string req = "";
            //Research
            req += "Vanilla Research: ";
            foreach (var res in research)
            {
                req += res.ToString() + "\n";
            }
            //TResearch
            req += "Tiberium Research:";
            foreach (var tres in tiberiumResearch)
            {

                req += tres.ToString() + "\n";
            }
            //Events
            req += "Events:";
            foreach (var ev in events)
            {

                req += ev.ToString() + "\n";
            }
            return req;
        }
    }
}
