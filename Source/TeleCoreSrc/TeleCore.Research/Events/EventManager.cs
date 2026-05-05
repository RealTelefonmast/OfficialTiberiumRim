using System;
using System.Collections.Generic;
using RimWorld.Planet;
using TeleCore.Defs;
using TeleCore.Types;
using TeleCore.Types.Exposables;
using TeleCore.Types.Structs;
using TeleCore.Types.Utils;
using Verse;
using World = RimWorld.Planet.World;

namespace TeleCore.Research.Events;

public class EventManager : WorldComponent
{
    public List<BaseEvent> allEvents = new();
    public Dictionary<EventDef, bool> currentEvents = new();
    public List<EventDef> newEvents = new();

    public EventManager(World world) : base(world)
    {
        newEvents = DefDatabase<EventDef>.AllDefsListForReading;
        GlobalEventHandler.ThingSpawned += CheckForEventStart;
        GlobalEventHandler.PawnHediffChanged += CheckForEventStartHediff;
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Collections.Look(ref newEvents, "newEvents");
        Scribe_Collections.Look(ref allEvents, "allEvents");
        Scribe_Collections.Look(ref currentEvents, "currentEvents");
    }

    public override void WorldComponentTick()
    {
        base.WorldComponentTick();
        for (var i = allEvents.Count - 1; i >= 0; i--)
        {
            var curEvent = allEvents[i];
            if (!currentEvents[curEvent.def]) curEvent.EventTick();
        }
    }

    private void CheckForEventStartHediff(PawnHediffChangedEventArgs args)
    {
        if (newEvents.NullOrEmpty()) return;
        for (var i = newEvents.Count - 1; i >= 0; i--)
        {
            var newEvent = newEvents[i];
            if (newEvent.triggerProps == null) continue;
            if (newEvent.triggerProps.TriggersEvent(args.Hediff, out var targets))
                StartEvent(newEvent, targets);
        }
    }

    private void CheckForEventStart(ThingStateChangedEventArgs args)
    {
        if (newEvents.NullOrEmpty()) return;
        for (var i = newEvents.Count - 1; i >= 0; i--)
        {
            var newEvent = newEvents[i];
            if (newEvent.triggerProps == null) continue;
            if (newEvent.triggerProps.TriggersEvent(args.Thing, out var targets)) StartEvent(newEvent, targets);
        }
    }

    public BaseEvent StartEvent(EventDef def, LookTargets targets = null)
    {
        if (def.IsActive)
        {
            Log.Warning("Trying to start event " + def.LabelCap + " which is already started!");
            return def.cachedEvent;
        }

        var baseEvent = (BaseEvent)Activator.CreateInstance(def.eventClass);
        baseEvent.StartEvent(def);
        baseEvent.EventTargets = targets;
        def.cachedEvent = baseEvent;
        allEvents.Add(baseEvent);
        currentEvents.Add(baseEvent.def, false);

        newEvents.Remove(def);
        return baseEvent;
    }

    public void ResetEvent(EventDef def)
    {
        //Remove from active pool
        allEvents.RemoveAll(e => e.def == def);
        currentEvents.Remove(def);

        //Add to remaining pool
        newEvents.Add(def);
    }

    public void Notify_EventFinished(BaseEvent baseEvent)
    {
        currentEvents[baseEvent.def] = true;
    }

    public bool IsActive(EventDef def)
    {
        return currentEvents.TryGetValue(def, out var value);
    }

    public bool IsFinished(EventDef def)
    {
        return currentEvents.TryGetValue(def, out var value) && value;
    }
}