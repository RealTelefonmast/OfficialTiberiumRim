// TODO: Decouple from TiberiumRim-specific types (TRUtils, TRLog, TRContent, TRCDefOf, TRThingDef)

using System;
using System.Collections.Generic;
using RimWorld;
using TeleCore.Events;
using TeleCore.GameData.Defs;
using TeleCore.Research.Defs;
using Verse;

namespace TeleCore.Research.Events;

public class DiscoveryList
{
    public List<DiscoveryDef> thingsToDiscover;

    public void Discover(string desc = default)
    {
        thingsToDiscover.ForEach(d => d.Discover());
    }
}

public class EventTriggerProperties
{
    public List<HediffDef> hediffs;
    public TargetProperties targets;

    public bool TriggersEvent<T>(T obj, out LookTargets lookTargets)
    {
        lookTargets = null;
        if (obj is Hediff hediff)
        {
            if (hediffs.NullOrEmpty()) return false;
            lookTargets = hediff.pawn;
            return hediffs.Contains(hediff.def);
        }

        if (obj is Thing thing)
        {
            if (targets == null) return false;
            lookTargets = thing;
            return targets.Accepts(thing);
        }

        return false;
    }
}

public class EventDef : Def
{
    public float activeDays = 0;

    [Unsaved] public BaseEvent cachedEvent;

    public DiscoveryList discoveries;

    public Type eventClass = typeof(BaseEvent);

    //TODO: Implement full use of incidentproperties
    public List<IncidentProperties> incidents;
    public LetterProperties letter;
    public EventTriggerProperties triggerProps;

    [Unsaved] public List<TResearchDef> unlocksResearch = new();

    public int ActiveTimeTicks => (int)(GenDate.TicksPerDay * activeDays);
    public bool Instant => ActiveTimeTicks == 0;
    public bool Activated => IsFinished || IsActive;
    public bool IsActive => TRUtils.EventManager().IsActive(this);
    public bool IsFinished => TRUtils.EventManager().IsFinished(this);

    public override void ResolveReferences()
    {
        foreach (var research in DefDatabase<TResearchDef>.AllDefs)
            if (research.requisites?.events?.Contains(this) ?? false)
                unlocksResearch.Add(research);
    }
}