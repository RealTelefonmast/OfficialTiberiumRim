// TODO: Decouple from TiberiumRim-specific types (TRUtils, TRLog, TRContent, TRCDefOf, TRThingDef)

using System.Collections.Generic;
using RimWorld;
using TeleCore.Research;
using TeleCore.Types;
using TeleCore.Types.Enums;
using TeleCore.Types.Exposables;
using TeleCore.Types.Utils;
using Verse;

namespace TeleCore.Defs;

public class TResearchDef : Def
{
    public List<EventDef> events;

    public TargetProperties mainTarget;

    [Unsaved] private TResearchGroupDef? parentGroup;

    public string projectDescription;
    public StatDef relevantPawnStat;
    public StatDef relevantTargetStat;

    public Requisites requisites;
    public string researchType = "missing";
    public List<SkillRequirement> skillRequirements;

    public List<TResearchTaskDef> tasks;
    public WorkTypeDef workType;

    public virtual bool RequisitesComplete => requisites?.FulFilled() ?? true;
    public virtual bool CanStartNow => RequisitesComplete;
    public bool IsFinished => ResearchRoot.Research.IsCompleted(this);
    public bool HasBeenSeen => ResearchRoot.DiscoveryTable.ResearchHasBeenSeen(this);

    public TResearchGroupDef ParentGroup
    {
        get
        {
            return parentGroup ??=
                DefDatabase<TResearchGroupDef>.AllDefsListForReading.FirstOrDefault(r =>
                    r.researchProjects.Contains(this));
        }
    }

    public virtual TResearchTaskDef CurrentTask
    {
        get { return tasks.FirstOrDefault(task => !task.IsFinished); }
    }

    public ResearchState State
    {
        get
        {
            if (Equals(ResearchRoot.Research.CurrentProject))
                return ResearchState.InProgress;
            if (IsFinished)
                return ResearchState.Finished;
            if (CanStartNow)
                return ResearchState.Available;
            return ResearchState.Hidden;
        }
    }

    public override void ResolveReferences()
    {
        base.ResolveReferences();
        //workType = DefDatabase<WorkTypeDef>.GetNamed("Research");
        //relevantPawnStat = StatDefOf.ResearchSpeed;
        //relevantTargetStat = StatDefOf.ResearchSpeedFactor;
    }

    //TODO: MULTIPLAYER
    //[SyncMethod(SyncContext.None)]
    public void TriggerEvents()
    {
        if (events.NullOrEmpty()) return;
        foreach (var @event in events)
            TRUtils.EventManager().StartEvent(@event);
    }


    public virtual void FinishAction()
    {
    }

    //TODO: MULTIPLAYER
    //[SyncMethod(SyncContext.None)]
    public void Debug_Finish()
    {
        TLog.Debug($"Force Finishing TResearch '{LabelCap}'");
        tasks.ForEach(t => t.Debug_Finish());
    }

    //[SyncMethod(SyncContext.None)]
    public void Debug_Reset()
    {
        if (!tasks.NullOrEmpty())
            tasks.ForEach(t => t.Debug_Reset());
        if (!events.NullOrEmpty())
            events.ForEach(t => TRUtils.EventManager().ResetEvent(t));
    }
}