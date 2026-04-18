using System.Collections.Generic;
using TeleCore.Defs;
using Verse;

namespace TeleCore.Unsorted;

//TODO: Add a visual readout of a graph between all roomcomps via interfaces

public class AtmosphericSystem : FlowSystem<RoomComponent, AtmosphericVolume, AtmosphericValueDef>
{
    private const bool enforceMinPipe = true;
    private const bool enforceMaxPipe = true;

    private readonly List<DefValue<AtmosphericValueDef, float>> _naturalAtmospheres = new();

    public AtmosphericVolume MapVolume { get; }

    public AtmosphericSystem(int mapCellSize)
    {
        _naturalAtmospheres = new List<DefValue<AtmosphericValueDef, float>>();

        //Map Volume is a special volume used for rooms that are outdoors
        MapVolume = new AtmosphericVolume(AtmosResources.DefaultAtmosConfigMap(mapCellSize));
        RegisterCustomVolume(MapVolume);

        Notify_Regenerate(mapCellSize);
    }

    public AtmosphericSystem(Map map) : this(map.cellIndices.NumGridCells)
    {
    }

    public void Init(Map map)
    {
        GenerateNaturalAtmospheres(map);

        //Add all natural atmospheres once via batch set
        var stack = new DefValueStack<AtmosphericValueDef, float>();
        foreach (var atmosphere in _naturalAtmospheres)
        {
            var storedOf = MapVolume.StoredValueOf(atmosphere.Def);
            var desired = MapVolume.CapacityPerType * atmosphere.Value;
            var diff = desired - storedOf;
            if (diff <= 0) continue;
            stack += (atmosphere, diff);
        }

        MapVolume.SetDirect(stack);
    }

    public void Notify_UpdateRoomComp(RoomComponent_Atmosphere comp)
    {
        if (Relations.TryGetValue(comp, out var volume) && volume != MapVolume)
            volume.UpdateVolume(comp.Room.CellCount);
    }

    protected override float GetInterfacePassThrough(TwoWayKey<RoomComponent> connectors)
    {
        var connA = connectors.A;
        var connB = connectors.B;
        if (connA == null)
        {
            TLog.Warning($"Tried to get interface pass-through for null connector: {connectors.A} -> {connectors.B}");
            return 1;
        }

        if (connA.CompNeighbors.Links.Count <= 0)
        {
            if (connA.Parent.IsOutside && connB.Parent.IsOutside)
                return 0; //Both outside, no flow needed
            TLog.Warning($"Tried to get interface pass-through with no links: {connectors.A} -> {connectors.B}");
            return 1;
        }

        var relLink = connectors.A.CompNeighbors.LinkFor((connectors.A, connectors.B));
        if (relLink == null)
        {
            TLog.Warning($"Tried to get interface pass-through with no links: {connectors.A} -> {connectors.B}");
            return 1;
        }

        return AtmosphericUtility.DefaultAtmosphericPassPercent(relLink.Connector);
    }

    protected override AtmosphericVolume CreateVolume(RoomComponent part)
    {
        var volume = new AtmosphericVolume(AtmosResources.DefaultAtmosConfig(part.Room.CellCount));
        volume.UpdateVolume(part.Room.CellCount);
        return volume;
    }

    public void Notify_AddRoomComp(RoomComponent_Atmosphere comp)
    {
        if (Relations.ContainsKey(comp))
        {
            TLog.Error("This technically shouldn't happen");
            return;
        }

        //Handle RoomComps that count as outdoors — share the map volume
        if (comp.IsOutdoors)
        {
            RegisterCustomRelation(comp, MapVolume);
            foreach (var adjComp in comp.CompNeighbors.Neighbors)
            {
                if (!Relations.TryGetValue(adjComp, out var adjVolume)) continue;
                var conn = new FlowInterface<RoomComponent, AtmosphericVolume, AtmosphericValueDef>(comp, adjComp,
                    MapVolume, adjVolume);
                AddInterface((comp, adjComp), conn);
            }

            AssertState();
            return;
        }

        //Add normal room->room connection
        var volume = GenerateForOrGetVolume(comp);
        foreach (var adjComp in comp.CompNeighbors.Neighbors)
        {
            if (!Relations.TryGetValue(adjComp, out var adjVolume)) continue;
            var conn = new FlowInterface<RoomComponent, AtmosphericVolume, AtmosphericValueDef>(comp, adjComp, volume,
                adjVolume);
            AddInterface((comp, adjComp), conn);
        }

        AssertState();
    }

    public void Notify_RemoveRoomComp(RoomComponent_Atmosphere comp)
    {
        if (!Relations.ContainsKey(comp)) return;

        if (Relations.TryGetValue(comp, out var volume))
        {
            TryRemoveRelatedPart(comp);
            if (volume == MapVolume)
                RemoveInterfacesWhere(iFace => iFace.FromPart == comp || iFace.ToPart == comp);

            AssertState();
        }
    }

    public void AssertState()
    {
    }

    #region Ticking

    protected override void PreTickProcessor(int tick)
    {
    }

    #endregion

    public void Notify_Regenerate(int cells)
    {
        MapVolume.UpdateVolume(cells);
    }

    public void Notify_InterfaceBetweenRoomsChanged(RoomComponent roomA, RoomComponent roomB, Thing thing,
        string signal)
    {
        if (InterfaceLookUp.TryGetValue((roomA, roomB), out var iFace))
            iFace.SetPassThrough(AtmosphericUtility.DefaultAtmosphericPassPercent(thing));
    }

    protected override DefValueStack<AtmosphericValueDef, float> FlowFunc(
        FlowInterface<RoomComponent, AtmosphericVolume, AtmosphericValueDef> iface,
        DefValueStack<AtmosphericValueDef, float> previous)
    {
        if (iface.FromPart.Parent.IsOutside && iface.ToPart.Parent.IsOutside) return previous;
        var from = iface.From;
        var to = iface.To;

        foreach (var vDef in from.AllowedValues)
        {
            var f = previous[vDef];
            var pressureDiff = SubPressure(from, vDef) - SubPressure(to, vDef);
            var src = f > 0 ? from : to;
            var contentDiff = System.Math.Abs((src.PrevStack.TotalValue - src.TotalValue).Value / src.MaxCapacity);
            f += pressureDiff * AtmosResources.CSquared;
            f *= 1 - AtmosResources.Friction;
            f *= 1 - GetTotalFriction(src);
            f *= 1 - 0.5f * contentDiff;
            f *= iface.PassPercent;
            previous[vDef] = f;
        }

        return previous;
    }

    protected override DefValueStack<AtmosphericValueDef, float> ClampFunc(
        FlowInterface<RoomComponent, AtmosphericVolume, AtmosphericValueDef> iface,
        DefValueStack<AtmosphericValueDef, float> flow, ClampType _)
    {
        //TODO: Implement per-type clamping
        return flow;
    }

    private static float SubPressure(AtmosphericVolume volume, AtmosphericValueDef value)
    {
        if (volume.CapacityPerType <= 0)
        {
            TLog.Warning($"Tried to get pressure from container with {volume.CapacityPerType} capacity!");
            return 0;
        }

        return volume.StoredValueOf(value) / volume.CapacityPerType * 100f;
    }

    public static float ClampFlow(float content, float flow, float limit)
    {
        if (content <= 0) return 0;
        if (flow >= 0) return flow <= limit ? flow : limit;
        return flow >= -limit ? flow : -limit;
    }

    public static float GetTotalFriction(AtmosphericVolume volume)
    {
        float totalFriction = 0;
        float totalVolume = 0;

        if (!volume.Stack.IsValid) return 0;
        foreach (var fluid in volume.Stack)
        {
            totalFriction += fluid.Def.friction * fluid.Value;
            totalVolume += fluid.Value;
        }

        if (totalVolume == 0) return 0;
        return totalFriction / totalVolume;
    }

    #region Natural Atmospheres

    private void GenerateNaturalAtmospheres(Map map)
    {
        if (!_naturalAtmospheres.NullOrEmpty()) return;

        var extension = map.Biome.GetModExtension<TAE_BiomeExtension>();
        var useRulesets = true;
        if (extension?.uniqueAtmospheres != null)
        {
            foreach (var atmosphere in extension.uniqueAtmospheres)
                _naturalAtmospheres.Add(atmosphere);
            useRulesets = false;
        }

        if (useRulesets)
            foreach (var ruleSet in DefDatabase<TAERulesetDef>.AllDefs)
            {
                if (ruleSet.realm == AtmosphericRealm.AnyBiome)
                {
                    if (ruleSet.atmospheres != null)
                        foreach (var floatRef in ruleSet.atmospheres)
                            _naturalAtmospheres.Add(floatRef);
                    continue;
                }

                if (ruleSet.realm == AtmosphericRealm.SpecificBiome)
                    if (ruleSet.biomes != null && ruleSet.biomes.Contains(map.Biome))
                        foreach (var atmosphere in ruleSet.atmospheres)
                            _naturalAtmospheres.Add(atmosphere);
            }

        foreach (var atmosphere in _naturalAtmospheres)
            if (atmosphere.Def.naturalOverlay != null)
            {
                TLog.Debug($"Adding Natural Overlay: {atmosphere.Def}");
                TeleUpdateManager.Notify_EnqueueNewSingleAction(() =>
                {
                    var newOverlay = new SkyOverlay_Atmosphere(atmosphere.Def.naturalOverlay);
                    //TODO: naturalOverlays.Add(newOverlay);
                });
            }
    }

    #endregion
}
