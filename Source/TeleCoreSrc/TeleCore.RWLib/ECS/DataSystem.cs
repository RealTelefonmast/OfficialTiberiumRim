using System.Collections.Generic;
using HarmonyLib;
using TeleCore.Events;
using TeleCore.Events.Args;
using Verse;

namespace TeleCore.RWLib.ECS;

//TODO: Implement component lookup for ECS-like systems

public static class DataSystem
{
    private static readonly Dictionary<int, (int,int)> _componentsByThing = new();
    private static List<IComponent> _components;
    
    static DataSystem()
    {
        GlobalEventHandler.Things.Spawned += OnThingSpawned;
        GlobalEventHandler.Things.Discarded += OnThingDiscarded;
    }

    private static void OnThingSpawned(ThingStateChangedEventArgs args)
    {
        var components = args.Thing.def.GetModExtension<DataComponentExtension>();
        if (components != null)
        {
            
        }
    }
    
    private static void OnThingDiscarded(ThingStateChangedEventArgs args)
    {
        
    }

    public static TComp GetComponent<TComp>(this Thing thing) where TComp : IComponent
    {
        if (_componentsByThing.TryGetValue(thing.thingIDNumber, out var pair))
        {
            var comp = _components[pair.Item1][pair.Item2];
            return (TComp)comp;
        }
        return default;
    }
}