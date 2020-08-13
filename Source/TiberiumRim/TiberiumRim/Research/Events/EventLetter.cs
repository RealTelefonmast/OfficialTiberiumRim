using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace TiberiumRim
{
    public class EventLetter : StandardLetter
    {
        private List<EventDef> events = new List<EventDef>();


        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref events, "events");
        }


        public override void Received()
        {
            base.Received();
        }

        public void AddEvents(List<EventDef> events)
        {
            this.events.AddRange(events);
        }

        public void AddEvent(EventDef eventDef)
        {
            events.Add(eventDef);
        }

        public override IEnumerable<DiaOption> Choices
        {
            get
            {
                foreach (var choice in base.Choices)
                {
                    yield return choice;
                }

                yield return new DiaOption("TR_OpenTab".Translate())
                {
                    action = delegate
                    {
                        Find.MainTabsRoot.SetCurrentTab(TiberiumDefOf.TiberiumTab);
                        var researchWindow = (MainTabWindow_TibResearch) Find.MainTabsRoot.OpenTab.TabWindow;
                        researchWindow.SelTab = ResearchTabOption.Projects;
                        var proj = events.First(e => !e.unlocksResearch.NullOrEmpty()).unlocksResearch.First();
                        researchWindow.SelProject = proj;

                        var manager = TRUtils.ResearchManager();
                        if (!manager.IsOpen(proj.ParentGroup))
                            manager.OpenClose(proj.ParentGroup);

                        //TODO: Select event
                    }
                };
            }
        }
    }
}
