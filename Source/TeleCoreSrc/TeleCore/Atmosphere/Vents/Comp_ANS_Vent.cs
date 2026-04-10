using RimWorld;
using TeleCore.Atmosphere.Defs;
using TeleCore.Atmosphere.Rooms;
using TeleCore.Events;
using TeleCore.Events.Args;
using TeleCore.FlowCore;
using TeleCore.Logging;
using TeleCore.Network.Flow;
using TeleCore.Network.Flow.Clamping;
using TeleCore.Network.Flow.Values;
using TeleCore.Primitive;
using TeleCore.Utils;
using Verse;

namespace TeleCore.Atmosphere.Vents;

public class Comp_ANS_Vent : Comp_AtmosphericNetworkStructure
{
    private CompFlickable _flickableComp;

    public CompProperties_ANS_Vent VentProps => (CompProperties_ANS_Vent)props;

    public IntVec3 IntakeCell { get; private set; }

    public bool IntakeCellBlocked => IntakeCell.GetEdifice(parent.Map) != null;

    //
    protected override Room AtmosphericSource => IntakeCell.GetRoom(parent.Map);

    //State Bools
    public bool CanTickNow
    {
        get
        {
            var isOwnedAtmosPartReady = OwnedAtmosPart.IsReady;
            var isNetworkWorkingOrPassiveVent = VentProps.passive || AtmosNetwork.IsWorking;
            var isPoweredOrPassiveVent = VentProps.passive || IsActive;
            return isOwnedAtmosPartReady && isNetworkWorkingOrPassiveVent && isPoweredOrPassiveVent;
        }
    }

    public bool IsActive => IsPowered && _flickableComp is null or { SwitchIsOn: true };

    public virtual bool CanManipulateNow => !IntakeCellBlocked;

    public double NextFlow { get; set; }
    public double PrevFlow { get; set; } = 0;
    public double Move { get; set; }
    public double FlowRate { get; set; }

    public DefValueStack<NetworkValueDef, double> PrevStackNetwork { get; set; }
    public DefValueStack<AtmosphericValueDef, double> PrevStackAtmos { get; set; }

    public override bool FX_ProvidesForLayer(FXArgs args)
    {
        if (args.categoryTag == "PoweredVent") return true;
        return base.FX_ProvidesForLayer(args);
    }

    public override float? FX_GetRotationSpeedOverride(FXLayerArgs args)
    {
        if (args.layerTag == "Blades" && IsActive) return 90;
        return base.FX_GetRotationSpeedOverride(args);
    }

    public override bool? FX_ShouldDraw(FXLayerArgs args)
    {
        if (_flickableComp is { SwitchIsOn: false }) return args.layerTag == "Closed";

        return args.layerTag != "Closed";

        return base.FX_ShouldDraw(args);
    }

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);
        _flickableComp = parent.GetComp<CompFlickable>();
        IntakeCell = VentProps.GetIntakePos(parent.Position, parent.Rotation);
    }

    public override void CompTick()
    {
        base.CompTick();
        if (!CanTickNow || !CanManipulateNow) return;

        var from = OwnedAtmosPart.Volume;
        var to = AtmosRoom.Volume;

        var flow = NextFlow;
        flow = FlowFunc(from, to, flow);
        NextFlow = flow;
        Move = flow;

        foreach (var value in Move)
            if (value > 0)
            {
                var move = System.Math.Abs(value.Value);
                if (from.TryRemove(value.Def.networkValue, move, out var result)) to.TryAdd(value.Def, result.Actual);
            }
            else if (value < 0)
            {
                var move = System.Math.Abs(value.Value);
                if (to.TryRemove(value.Def, move, out var result)) from.TryAdd(value.Def.networkValue, result.Actual);
            }
    }

    private static double SubPressure<Tvalue>(FlowVolumeBase<Tvalue> volume, Tvalue value) where Tvalue : FlowValueDef
    {
        if (volume.CapacityPerType <= 0)
        {
            TLog.Warning($"Tried to get pressure from container with {volume.CapacityPerType} capacity!");
            return 0;
        }

        return volume.StoredValueOf(value) / volume.CapacityPerType * 100d;
    }


    private DefValueStack<AtmosphericValueDef, double> ClampFunc(NetworkVolume netVolume, AtmosphericVolume atmosVolume,
        DefValueStack<AtmosphericValueDef, double> previous, ClampType clampType)
    {
        var from = netVolume;
        var to = atmosVolume;

        var d0 = 1d / 2;
        var d1 = 1d / 2;

        foreach (var vDef in to.AllowedValues)
        {
            var f = previous[vDef];
            var clamped = AtmosphericSystem.ClampFunc(d0, d1, from.StoredValueOf(vDef.networkValue),
                to.StoredValueOf(vDef), from.CapacityPerType, to.CapacityPerType, f.Value);
            previous[vDef] = (f.Def, clamped);
        }

        return previous;
    }


    //Helpers
    //Pressure check hack, if other container has higher room pressure it can send to this current vent
    private bool NeedsToReceiveFrom(Comp_AtmosphericNetworkStructure other)
    {
        return VentProps.ventMode switch
        {
            AtmosphericVentMode.Intake => false,
            AtmosphericVentMode.Output => true,
            AtmosphericVentMode.TwoWay => AtmosRoom.Volume.FillPercent < other.AtmosRoom.Volume.FillPercent,
            _ => false
        };
    }

    //Check whether we have vent neighbours that can receive
    private bool IsAtmosphericProvider()
    {
        if (OwnedAtmosPart.Network.Graph.TryGetAdjacencyList(OwnedAtmosPart, out var adjacencyList))
        {
            if (!adjacencyList.Any()) return false;
            return adjacencyList.Any(c =>
                c.Item2.Value.Parent is Comp_ANS_Vent pvent && pvent.NeedsToReceiveFrom(this));
        }

        return false;
    }

    #region Handle Network->Room

    private DefValueStack<AtmosphericValueDef, double> NextFlow;
    private DefValueStack<AtmosphericValueDef, double> Move;

    private DefValueStack<AtmosphericValueDef, double> FlowFunc(NetworkVolume netVolume, AtmosphericVolume atmosVolume,
        DefValueStack<AtmosphericValueDef, double> previous)
    {
        var from = netVolume;
        var to = atmosVolume;

        foreach (var vDef in to.AllowedValues)
        {
            var f = previous[vDef];
            var pressureDiff = SubPressure(from, vDef.networkValue) - SubPressure(to, vDef);
            var src = f > 0
                ? (from.PrevStack.TotalValue, from.TotalValue, from.MaxCapacity)
                : (to.PrevStack.TotalValue, to.TotalValue, to.MaxCapacity);
            var contentDiff = System.Math.Abs((src.Item1 - src.Item2).Value / src.MaxCapacity);
            f += pressureDiff * AtmosResources.CSquared;
            f *= 1 - AtmosResources.Friction;
            f *= 1 - 0.5 * contentDiff; //DampFriction
            previous[vDef] = f;
        }

        return previous;
    }

    #endregion
}