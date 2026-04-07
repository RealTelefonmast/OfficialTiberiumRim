using System.Collections.Generic;
using TeleCore.Absorbed.Loader.Update;
using TeleCore.Atmosphere.Defs;
using TeleCore.Atmosphere.Rendering;
using TeleCore.Atmosphere.Rooms;
using TeleCore.Atmosphere.Utils;
using TeleCore.Defs;
using TeleCore.FlowCore;
using TeleCore.Generics;
using TeleCore.Logging;
using TeleCore.Network.Flow.Clamping;
using TeleCore.Primitive;
using TeleCore.Rendering;
using TeleCore.Systems.RoomTracking;
using TeleCore.Utils;
using UnityEngine;
using Verse;

namespace TeleCore.Atmosphere;

//TODO: Add a visual readout of a graph between all roomcomps via interfaces
//TODO: Like those burst graphs

public abstract class AtmosphericSystem : FlowSystem<RoomComponent, AtmosphericVolume, AtmosphericValueDef>
{
    private AtmosphericVolume _mapVolume;
    private readonly List<DefValue<AtmosphericValueDef, float>> _naturalAtmospheres = new();
    //Note: Interface represents a portal between rooms

    public AtmosphericVolume MapVolume => _mapVolume;

    //Note: Friction is key!!
    public static double Friction => 0.01;
    public static double CSquared => 0.03;
    public static double DampFriction => 0.01; //TODO: Extract into global flowsystem config xml or mod settings

    public AtmosphericSystem(int mapCellSize)
    {
        _naturalAtmospheres = new List<DefValue<AtmosphericValueDef, float>>();

        //Map Volume is a special volume used for rooms that are outdoors
        _mapVolume = new AtmosphericVolume(AtmosResources.DefaultAtmosConfigMap(mapCellSize));
        RegisterCustomVolume(_mapVolume);

        //
        Notify_Regenerate(mapCellSize);
    }

    public AtmosphericSystem(Map map) : this(map.cellIndices.NumGridCells)
    {
    }

    public void Init(Map map)
    {
        GenerateNaturalAtmospheres(map);

        //Add all natural atmospheres once
        var stack = new DefValueStack<AtmosphericValueDef, double>();
        foreach (var atmosphere in _naturalAtmospheres)
        {
            var storedOf = _mapVolume.StoredValueOf(atmosphere.Def);
            var desired = _mapVolume.CapacityPerType * atmosphere.Value;
            var diff = System.Math.Round(desired - storedOf, 6);
            if (diff <= 0) continue;
            stack += (atmosphere,diff);
            //_mapVolume.TryAdd(atmosphere, diff);
        }
        _mapVolume.SetDirect(stack);
    }

    public void Notify_UpdateRoomComp(RoomComponent_Atmosphere comp)
    {
        if (Relations.TryGetValue(comp, out var volume) && volume != _mapVolume)
        {
            volume.UpdateVolume(comp.Room.CellCount);
        }
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
            {
                //Both are outside, no flow needed.
                return 0;
            }
            global::TeleCore.Logging.TLog.Warning($"Tried to get interface pass-through with no links: {connectors.A} -> {connectors.B}");
            return 1;
        }

        RoomComponentLink? relLink = connectors.A.CompNeighbors.LinkFor((connectors.A, connectors.B));
        if (relLink == null)
        {
            TLog.Warning($"Tried to get interface pass-through with no links: {connectors.A} -> {connectors.B}");
            return 1;
        }
        return AtmosphericUtility.DefaultAtmosphericPassPercent(relLink.Connector);
    }

    protected override AtmosphericVolume CreateVolume(RoomComponent part)
    {
        var volume =  new AtmosphericVolume(AtmosResources.DefaultAtmosConfig(part.Room.CellCount));
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

        //Handle RoomComps that count as outdoors
        //Should use the same container as the "map" room
        if (comp.IsOutdoors)
        {
            RegisterCustomRelation(comp, _mapVolume);
            foreach (var adjComp in comp.CompNeighbors.Neighbors)
            {
                if (!Relations.TryGetValue(adjComp, out var adjVolume)) continue;

                //Connect map volume to adjacent volume of a room that counts as outdoors
                var conn = new FlowInterface<RoomComponent, AtmosphericVolume, AtmosphericValueDef>(comp, adjComp, _mapVolume, adjVolume);
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
            var conn = new FlowInterface<RoomComponent, AtmosphericVolume, AtmosphericValueDef>(comp, adjComp, volume, adjVolume);
            AddInterface((comp, adjComp), conn);
        }
        AssertState();
        //PushNaturalSaturation();
    }

    public void Notify_RemoveRoomComp(RoomComponent_Atmosphere comp)
    {
        if (!Relations.ContainsKey(comp)) return;

        if (Relations.TryGetValue(comp, out var volume))
        {
            if (volume == _mapVolume)
            {
                TryRemoveRelatedPart(comp);
                RemoveInterfacesWhere(iFace => iFace.FromPart == comp || iFace.ToPart == comp);
            }
            else
            {
                TryRemoveRelatedPart(comp);
            }
            AssertState();
        }
    }

    private const char check = '✓';
    private const char fail = 'X';

    private string Icon(bool checkFail)
    {
        return $"{(checkFail ? check : fail)}";
    }

    private Color ColorSel(bool checkFail)
    {
        return checkFail ? Color.green : Color.red;
    }

    public void AssertState()
    {

    }

    #region Ticking

    protected override void PreTickProcessor(int tick)
    {
        //Keep Natural Saturation Up
        // if (tick % 250 == 0)
        // {
        //     PushNaturalSaturation();
        // }
    }

    #endregion

    public void Notify_Regenerate(int cells)
    {
        _mapVolume.UpdateVolume(cells);
    }

    public void Notify_InterfaceBetweenRoomsChanged(RoomComponent roomA, RoomComponent roomB, Thing thing, string signal)
    {
        if (InterfaceLookUp.TryGetValue((roomA, roomB), out var iFace))
        {
            iFace.SetPassThrough(AtmosphericUtility.DefaultAtmosphericPassPercent(thing));
        }
    }

    protected override DefValueStack<AtmosphericValueDef, double> FlowFunc(
        FlowInterface<RoomComponent, AtmosphericVolume, AtmosphericValueDef> iface,
        DefValueStack<AtmosphericValueDef, double> previous)
    {
        if(iface.FromPart.Parent.IsOutside && iface.ToPart.Parent.IsOutside) return previous;
        var from = iface.From;
        var to = iface.To;

        foreach (var vDef in from.AllowedValues)
        {
            var f = previous[vDef];
            var pressureDiff = SubPressure(from, vDef) - SubPressure(to, vDef); // pressure differential
            var src = f > 0 ? from : to;
            var contentDiff = System.Math.Abs((src.PrevStack.TotalValue - src.TotalValue).Value/src.MaxCapacity);
            f += pressureDiff * AtmosResources.CSquared;
            f *= 1 - AtmosResources.Friction;
            f *= 1 - GetTotalFriction(src); //Additional Friction from each fluid/gas
            f *= 1 - 0.5 * contentDiff; //DampFriction
            f *= iface.PassPercent;
            previous[vDef] = f;
        }
        return previous;
    }

    /*
    protected override double FlowFunc(FlowInterface<RoomComponent, AtmosphericVolume, AtmosphericValueDef> iface, double f)
    {
        var from = iface.From;
        var to = iface.To;

        var dp = Pressure(from) - Pressure(to); // pressure differential
        var src = f > 0 ? from : to;
        var dc = Math.Max(0, src.PrevStack.TotalValue - src.TotalValue);
        f += dp * CSquared;
        f *= 1 - Friction;
        f *= 1 - GetTotalFriction(src); //Additional Friction from each fluid/gas
        f *= 1 - Math.Min(0.5, DampFriction * dc);
        return f;
    }
    */

    private const bool enforceMinPipe = true;
    private const bool enforceMaxPipe = true;

    protected override DefValueStack<AtmosphericValueDef, double> ClampFunc(FlowInterface<RoomComponent, AtmosphericVolume, AtmosphericValueDef> iface, DefValueStack<AtmosphericValueDef, double> flow, ClampType _)
    {
        return flow;
        var from = iface.From;
        var to = iface.To;

        var d0 = 1d / System.Math.Max(1, Connections[from].Count);
        var d1 = 1d / System.Math.Max(1, Connections[to].Count);

        foreach (var vDef in from.AllowedValues)
        {
            var f = flow[vDef];
            var clamped = ClampFunc(d0, d1, from.StoredValueOf(vDef), to.StoredValueOf(vDef), from.CapacityPerType, to.CapacityPerType, f.Value);
            flow[vDef] = (f.Def, clamped);
        }

        return flow;
    }

    internal static double ClampFunc(double d0, double d1, double fromTotal, double toTotal, double fromCap, double toCap,  double f)
    {
        if (enforceMinPipe)
        {
            double c;
            if (f > 0)
            {
                c = fromTotal;
                f = ClampFlow(c, f, d0 * c);
            }
            else if (f < 0)
            {
                c = toTotal;
                f = -ClampFlow(c, -f, d1 * c);
            }
        }

        if (enforceMaxPipe)
        {
            double r;
            if (f > 0)
            {
                r = fromCap - toTotal;
                f = ClampFlow(r, f, d1 * r);
            }
            else if (f < 0)
            {
                r = toCap - fromTotal;
                f = -ClampFlow(r, -f, d0 * r);
            }
        }

        return f;
    }

    private static double SubPressure(AtmosphericVolume volume, AtmosphericValueDef value)
    {
        if (volume.CapacityPerType <= 0)
        {
            TLog.Warning($"Tried to get pressure from container with {volume.MaxCapacity} capacity!");
            return 0;
        }
        return volume.StoredValueOf(value) / volume.CapacityPerType * 100d;
    }

    public static double ClampFlow(double content, double flow, double limit)
    {
        if (content <= 0) return 0;

        if (flow >= 0) return flow <= limit ? flow : limit;
        return flow >= -limit ? flow : -limit;
    }

    public static double GetTotalFriction(AtmosphericVolume volume)
    {
        double totalFriction = 0;
        double totalVolume = 0;

        if (!volume.Stack.IsValid) return 0;
        foreach (var fluid in volume.Stack)
        {
            totalFriction += fluid.Def.friction * fluid.Value;
            totalVolume += fluid.Value;
        }

        if (totalVolume == 0) return 0;

        var averageFriction = totalFriction / totalVolume;
        return averageFriction;
    }

    #region Atmospheric

    /*private void PushNaturalSaturation()
    {
        foreach (var atmosphere in _naturalAtmospheres)
        {
            var storedOf = _mapVolume.StoredValueOf(atmosphere.Def);
            var desired = _mapVolume.CapacityPerType * atmosphere.Value;
            var diff = Math.Round(desired - storedOf, 6);
            if (diff <= 0) continue;
            //TLog.Debug($"Pushing {diff}/{_mapVolume.CapacityPerType} for {atmosphere.Def}: {Math.Round(pct, 6)}");
            _mapVolume.TryAdd(atmosphere, diff);
        }
    }*/

    //Note: Called once to generate all natural atmospheres
    private void GenerateNaturalAtmospheres(Map map)
    {
        if (!_naturalAtmospheres.NullOrEmpty()) return;

        var extension = map.Biome.GetModExtension<TAE_BiomeExtension>();
        var useRulesets = true;
        if (extension?.uniqueAtmospheres != null)
        {
            foreach (var atmosphere in extension.uniqueAtmospheres)
            {
                _naturalAtmospheres.Add(atmosphere);
                //TODO: MapVolume.Data_RegisterSourceType(atmosphere.Def);
            }

            useRulesets = false;
        }

        if (useRulesets)
        {
            foreach (var ruleSet in DefDatabase<TAERulesetDef>.AllDefs)
            {
                if (ruleSet.realm == AtmosphericRealm.AnyBiome)
                {
                    if (ruleSet.atmospheres != null)
                    {
                        foreach (var floatRef in ruleSet.atmospheres)
                        {
                            _naturalAtmospheres.Add(floatRef);
                        }
                    }
                    continue;
                }

                if (ruleSet.realm == AtmosphericRealm.SpecificBiome)
                {
                    if (ruleSet.biomes != null)
                    {
                        if (ruleSet.biomes.Contains(map.Biome))
                        {
                            foreach (var atmosphere in ruleSet.atmospheres)
                            {
                                _naturalAtmospheres.Add(atmosphere);
                                //TODO: mapContainer.Data_RegisterSourceType(atmosphere.Def);
                            }
                        }
                    }
                }
            }
        }

        foreach (var atmosphere in _naturalAtmospheres)
        {
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
    }

    #endregion
}
