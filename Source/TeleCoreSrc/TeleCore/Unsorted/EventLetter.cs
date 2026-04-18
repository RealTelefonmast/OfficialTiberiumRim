// TODO: Decouple from TiberiumRim-specific types (TRUtils, TRLog, TRContent, TRCDefOf, TRThingDef)

using System.Collections.Generic;
using System.Linq;
using TeleCore.Defs;
using Verse;

namespace TeleCore.Unsorted;

public class EventLetter : StandardLetter
{
    private List<EventDef> events = new();

    public override IEnumerable<DiaOption> Choices
    {
        get
        {
            foreach (var choice in base.Choices) yield return choice;

            yield return new DiaOption("TR_OpenTab".Translate())
            {
                action = delegate
                {
                    Find.MainTabsRoot.SetCurrentTab(TRCDefOf.TiberiumTab);
                    var researchWindow = (MainTabWindow_TResearch)Find.MainTabsRoot.OpenTab.TabWindow;
                    researchWindow.SelTab = ResearchTabOption.Projects;
                    var proj = Enumerable.First<TResearchDef>(events.First(e => !GenList.NullOrEmpty<TResearchDef>(e.unlocksResearch)).unlocksResearch);
                    researchWindow.SetProject(proj);

                    var manager = ResearchRoot.Research;
                    if (!manager.IsOpen(proj.ParentGroup))
                        manager.OpenClose(proj.ParentGroup);

                    //TODO: Select event
                }
            };
        }
    }


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
}