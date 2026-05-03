using System.Collections.Generic;
using TeleCore.Defs;
using Verse;

namespace TeleCore.Unsorted;

public class ThingGroupCacheInfo : MapInformation
{
    private readonly Dictionary<ThingGroupDef, List<ThingWithComps>> cachedCompParentsByGroup = new();
    private readonly Dictionary<ThingGroupDef, List<ThingComp>> cachedCompsByGroup = new();
    private readonly Dictionary<ThingGroupDef, List<Thing>> cachedThingsByGroup = new();

    public ThingGroupCacheInfo(Map map) : base(map)
    {
        GlobalEventHandler.Things.Spawned += OnRegisterPart;
        GlobalEventHandler.Things.Despawning += OnDeregisterPart;
    }

    public List<Thing> ThingsOfGroup(ThingGroupDef group)
    {
        return cachedThingsByGroup.TryGetValue(group);
    }

    public List<ThingWithComps> GetCompParentsFromGroup(ThingGroupDef group)
    {
        if (group == null) return null;
        return cachedCompParentsByGroup.TryGetValue(group);
    }

    public List<ThingComp> GetCompsFromGroup(ThingGroupDef group)
    {
        return cachedCompsByGroup.TryGetValue(group);
    }

    //
    private void OnRegisterPart(ThingStateChangedEventArgs args)
    {
        var thing = args.Thing;
        if (thing.def.HasTeleExtension(out var extension))
            foreach (var group in extension.thingGroups.groups)
                RegisterThingGroup(group, thing);
    }

    private void OnDeregisterPart(ThingStateChangedEventArgs args)
    {
        var thing = args.Thing;
        if (thing.def.HasTeleExtension(out var extension))
            foreach (var group in extension.thingGroups.groups)
                DeregisterThingGroup(group, thing);
    }

    private void RegisterThingGroup(ThingGroupDef groupDef, object obj)
    {
        if (groupDef == null) return;
        switch (obj)
        {
            case Thing thing:
            {
                if (!cachedThingsByGroup.ContainsKey(groupDef)) cachedThingsByGroup.Add(groupDef, new List<Thing>());
                cachedThingsByGroup[groupDef].Add(thing);

                break;
            }
            case ThingComp comp:
            {
                if (!cachedCompsByGroup.ContainsKey(groupDef)) cachedCompsByGroup.Add(groupDef, new List<ThingComp>());
                cachedCompsByGroup[groupDef].Add(comp);

                //Cache Parent
                if (!cachedCompParentsByGroup.ContainsKey(groupDef))
                    cachedCompParentsByGroup.Add(groupDef, new List<ThingWithComps>());
                cachedCompParentsByGroup[groupDef].Add(comp.parent);
                break;
            }
        }

        //
        if (groupDef.parentGroup != null)
            RegisterThingGroup(groupDef.parentGroup, obj);
    }

    private void DeregisterThingGroup(ThingGroupDef groupDef, object obj)
    {
        if (groupDef == null) return;

        switch (obj)
        {
            case Thing thing:
            {
                cachedThingsByGroup[groupDef].Remove(thing);
                break;
            }
            case ThingComp comp:
            {
                cachedCompsByGroup[groupDef].Remove(comp);
                cachedCompParentsByGroup[groupDef].Remove(comp.parent);
                break;
            }
        }

        //
        if (groupDef.parentGroup != null)
            DeregisterThingGroup(groupDef.parentGroup, obj);
    }
}