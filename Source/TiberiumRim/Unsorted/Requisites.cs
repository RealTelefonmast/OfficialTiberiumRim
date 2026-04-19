using System.Collections.Generic;
using System.Linq;
using Verse;

namespace TiberiumRim;

public class Requisites
{
    public List<EventDef> events = new();
    public bool eventsMustBeFinished = true;
    public List<ResearchProjectDef> research = new();
    public List<TResearchDef> tiberiumResearch = new();
    public List<TResearchTaskDef> tiberiumResearchTasks = new();

    //public bool Completed => ResearchComplete && TResearchComplete && EventsHappened;
    private bool ResearchComplete => research.NullOrEmpty() || research.All(r => r.IsFinished);
    private bool TResearchComplete => tiberiumResearch.NullOrEmpty() || tiberiumResearch.All(tr => tr.IsFinished);

    private bool TResearchTasksComplete =>
        tiberiumResearchTasks.NullOrEmpty() || tiberiumResearchTasks.All(t => t.IsFinished);

    private bool EventsHappened => events.NullOrEmpty() || eventsMustBeFinished
        ? events.All(e => e.IsFinished)
        : events.All(e => e.IsActive);

    public bool FulFilled()
    {
        return ResearchComplete && TResearchComplete && TResearchTasksComplete && EventsHappened;
    }

    public string MissingString()
    {
        string missing = "TR_MissingRequisite".Translate();
        //Research
        var vanResearch = research.Where(res => !res.IsFinished)
            .Aggregate("", (current, res) => current + ("\n" + res));
        if (!vanResearch.NullOrEmpty())
            missing += "\n" + vanResearch;
        //TResearch
        var tibResearch = tiberiumResearch.Where(res => !res.IsFinished)
            .Aggregate("", (current, res) => current + ("\n" + res));
        if (!tibResearch.NullOrEmpty())
            missing += "\n" + tibResearch;
        //TResearchTask
        var tasks = tiberiumResearchTasks.Where(task => !task.IsFinished)
            .Aggregate("", (current, task) => current + ("\n" + task));
        if (!tasks.NullOrEmpty())
            missing += "\n" + tasks;
        //Events
        var eventsString = events.Where(@event => !@event.IsFinished)
            .Aggregate("", (current, @event) => current + ("\n" + @event));
        if (!eventsString.NullOrEmpty())
            missing += "\n" + eventsString;

        return missing;
    }

    public override string ToString()
    {
        var req = "";
        //Research
        req += "Vanilla Research: ";
        foreach (var res in research) req += res + "\n";
        //TResearch
        req += "Tiberium Research:";
        foreach (var tres in tiberiumResearch) req += tres + "\n";
        //TResearchTask
        req += "Tiberium Research Tasks:";
        foreach (var task in tiberiumResearchTasks) req += task + "\n";
        //Events
        req += "Events:";
        foreach (var ev in events) req += ev.ToString() + "\n";
        return req;
    }
}