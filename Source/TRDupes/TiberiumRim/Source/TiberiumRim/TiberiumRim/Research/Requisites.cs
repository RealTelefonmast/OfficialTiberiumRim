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
        public List<TResearchTaskDef> tiberiumResearchTasks = new List<TResearchTaskDef>();
        public List<EventDef> events = new List<EventDef>();
        public bool eventsMustBeFinished = true;

        //public bool Completed => ResearchComplete && TResearchComplete && EventsHappened;
        private bool ResearchComplete => research.NullOrEmpty() || research.All(r => r.IsFinished);
        private bool TResearchComplete => tiberiumResearch.NullOrEmpty() || tiberiumResearch.All(tr => tr.IsFinished);
        private bool TResearchTasksComplete => tiberiumResearchTasks.NullOrEmpty() || tiberiumResearchTasks.All(t => t.IsFinished);

        private bool EventsHappened => events.NullOrEmpty() || eventsMustBeFinished ? events.All(e => e.IsFinished) : events.All(e => e.IsActive);

        public bool FulFilled()
        {
            return ResearchComplete && TResearchComplete && TResearchTasksComplete && EventsHappened;
        }

        public string MissingString()
        {
            string missing = "TR_MissingRequisite".Translate();
            //Research
            string vanResearch = research.Where(res => !res.IsFinished).Aggregate("", (current, res) => current + ("\n" + res));
            if (!vanResearch.NullOrEmpty())
                missing += "\n" + vanResearch;
            //TResearch
            string tibResearch = tiberiumResearch.Where(res => !res.IsFinished).Aggregate("", (current, res) => current + ("\n" + res));
            if (!tibResearch.NullOrEmpty())
                missing += "\n" + tibResearch;
            //TResearchTask
            string tasks = tiberiumResearchTasks.Where(task => !task.IsFinished).Aggregate("", (current, task) => current + ("\n" + task));
            if (!tasks.NullOrEmpty())
                missing += "\n" + tasks;
            //Events
            string eventsString = events.Where(@event => !@event.IsFinished).Aggregate("", (current, @event) => current + ("\n" + @event));
            if (!eventsString.NullOrEmpty())
                missing += "\n" + eventsString;

            return missing;
        }

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
            //TResearchTask
            req += "Tiberium Research Tasks:";
            foreach (var task in tiberiumResearchTasks)
            {

                req += task.ToString() + "\n";
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
