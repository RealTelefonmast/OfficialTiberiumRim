using System;
using Verse;

namespace TeleCore.Unsorted;

/// <summary>
/// </summary>
public class ThingTrackerMapInfo : MapInformation
{
    private readonly ThingTrackerComp[] trackerComps;

    public ThingTrackerMapInfo(Verse.Map map) : base(map)
    {
        var compTypes = typeof(ThingTrackerComp).AllSubclassesNonAbstract();
        trackerComps = new ThingTrackerComp[compTypes.Count];
        for (var i = 0; i < compTypes.Count; i++)
        {
            var type = compTypes[i];
            try
            {
                trackerComps[i] = (ThingTrackerComp)Activator.CreateInstance(type, this);
            }
            catch (Exception ex)
            {
                TLog.Error($"Could not instantiate {nameof(ThingTrackerComp)} of type {type}:\n{ex}");
            }
        }
    }
}

/// <summary>
///     Provides an abstract base for custom Thing-tracking worker classes which process Thing-Data on Spawn/Despawn/State
///     change events
/// </summary>
public abstract class ThingTrackerComp
{
    protected ThingTrackerMapInfo parent;

    protected ThingTrackerComp(ThingTrackerMapInfo parent)
    {
        this.parent = parent;
        GlobalEventHandler.Things.Spawned += Notify_ThingRegistered;
        GlobalEventHandler.Things.Despawning += Notify_ThingDeregistered;
        GlobalEventHandler.Things.SentSignal += Notify_ThingSentSignal;
    }

    public abstract void Notify_ThingRegistered(ThingStateChangedEventArgs args);
    public abstract void Notify_ThingDeregistered(ThingStateChangedEventArgs args);
    public abstract void Notify_ThingSentSignal(ThingStateChangedEventArgs args);
}