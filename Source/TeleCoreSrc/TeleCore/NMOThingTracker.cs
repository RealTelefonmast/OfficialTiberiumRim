using JetBrains.Annotations;
using RimWorld;
using TeleCore.Atmosphere.Oxygen.MapData;
using TeleCore.Events;
using TeleCore.Events.Args;
using TeleCore.GameData;
using TeleCore.Utils;
using Verse;

namespace TeleCore;

public class NMOThingTracker : ThingTrackerComp
{
    public NMOThingTracker([NotNull] ThingTrackerMapInfo parent) : base(parent)
    {
    }

    public override void Notify_ThingRegistered(ThingStateChangedEventArgs args)
    {
        switch (args.Thing)
        {
            case ThingWithComps twc when OxygenUtility.IsBurner(twc):
                twc.Map.GetMapInfo<AirMapInfo>().RegisterBurner(twc);
                break;
            case Fire fire:
                fire.Map.GetMapInfo<AirMapInfo>().RegisterFire(fire);
                break;
        }
    }

    public override void Notify_ThingDeregistered(ThingStateChangedEventArgs args)
    {
        switch (args.Thing)
        {
            case ThingWithComps twc when OxygenUtility.IsBurner(twc):
                twc.Map.GetMapInfo<AirMapInfo>().Deregister(twc);
                break;
            case Fire fire:
                fire.Map.GetMapInfo<AirMapInfo>().Deregister(fire);
                break;
        }
    }

    public override void Notify_ThingSentSignal(ThingStateChangedEventArgs args)
    {
    }
}