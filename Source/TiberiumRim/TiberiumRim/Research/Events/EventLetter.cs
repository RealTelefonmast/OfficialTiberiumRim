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
        private List<EventDef> events;

        public void AddEvents(List<EventDef> events)
        {
            this.events = events;
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
                        MainTabWindow_TibResearch.SelTab = ResearchTabOption.Projects;
                        var proj = events.First(e => !e.unlocksResearch.NullOrEmpty()).unlocksResearch.First();
                        var manager = TRUtils.ResearchManager();
                        MainTabWindow_TibResearch.SelProject = proj;
                        if (!manager.IsOpen(proj.ParentGroup))
                            manager.OpenClose(proj.ParentGroup);

                        //TODO: Select event
                    }
                };
            }
        }
    }
}
