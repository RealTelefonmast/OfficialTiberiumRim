using RimWorld;
using TeleCore.FlowCore.Containers;
using TeleCore.Logging;
using TeleCore.Utils;
using UnityEngine;
using Verse;

namespace TeleCore.Atmosphere.Vents;

public class Comp_ANS_VentBase : Comp_AtmosphericNetworkStructure
{
    private CompFlickable _flickableComp;

    public CompProperties_ANS_Vent VentProps => (CompProperties_ANS_Vent)props;

    public IntVec3 IntakeCell { get; private set; }

    public bool IntakeCellBlocked => IntakeCell.GetEdifice(parent.Map) != null;

    //
    protected override Room AtmosphericSource => IntakeCell.GetRoom(parent.Map);

    //State Bools
    public bool CanTickNow => IsPowered;
    public virtual bool CanManipulateNow => !IntakeCellBlocked;


    protected bool CanWork_Obsolete
    {
        get
        {
            if (!IsPowered) return false;
            foreach (var def in VentProps.AllowedValues)
                switch (VentProps.ventMode)
                {
                    case AtmosphericVentMode.Intake:
                        if (AtmosRoom.Container.StoredValueOf(def) <= 0) return false;
                        if (AtmosNetwork.Container.Full) return false;
                        break;
                    case AtmosphericVentMode.Output:
                        if (AtmosRoom.Container.StoredValueOf(def) >= 1) return false;
                        if (AtmosNetwork.Container.Empty) return false;
                        break;
                    case AtmosphericVentMode.TwoWay:
                        break;
                }

            return true;
        }
    }


    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);
        _flickableComp = parent.GetComp<CompFlickable>();
        IntakeCell = VentProps.GetIntakePos(parent.Position, parent.Rotation);
        TLog.Debug(
            $"Created vent with allowed values: {VentProps.AllowedValues.ToStringSafeEnumerable()} and\n{AtmosNetwork.Props.containerConfig.AllowedValues}");
    }

    public override void CompTick()
    {
        base.CompTick();
        if (CanTickNow && CanManipulateNow)
            if (TryManipulateAtmosphere(1))
            {
            }
    }

    private void TryEqualize(AtmosphericDef atmosphericDef, int tickRate = 1)
    {
        var roomComp = AtmosRoom;
        var roomContainer = roomComp.CurrentContainer;
        var networkComp = AtmosNetwork;

        if (FlowValueUtils.NeedsEqualizing(roomContainer, networkComp.Container, out var flowDir, out var diffPct))
        {
            diffPct = Mathf.Abs(diffPct);
            switch (flowDir)
            {
                //Push From Room Into Vent
                case ValueFlowDirection.Positive:
                {
                    var value = roomContainer.StoredValueOf(atmosphericDef) * 0.5f;
                    var flowAmount = networkComp.Container.GetMaxTransferRate(atmosphericDef.networkValue,
                        Mathf.CeilToInt(value * diffPct * atmosphericDef.networkValue.FlowRate));
                    if (roomContainer.TryTransferTo(networkComp.Container, atmosphericDef, Mathf.Round(flowAmount)))
                    {
                        //...
                    }

                    break;
                }
                //Push From Vent Into Room
                case ValueFlowDirection.Negative:
                {
                    if (IsAtmosphericProvider()) return;

                    var value = networkComp.Container.StoredValueOf(atmosphericDef.networkValue) * 0.5f;
                    var flowAmount = networkComp.Container.GetMaxTransferRate(atmosphericDef.networkValue,
                        Mathf.CeilToInt(value * diffPct * atmosphericDef.networkValue.FlowRate));
                    if (roomContainer.TryReceiveFrom(networkComp.Container, atmosphericDef,
                            Mathf.RoundToInt(flowAmount)))
                    {
                        //...
                    }

                    break;
                }
            }
        }
    }

    private bool TryManipulateAtmosphere(int tick)
    {
        var totalThroughput = VentProps.gasThroughPut * tick;
        foreach (var def in VentProps.AllowedValues)
            switch (VentProps.ventMode)
            {
                //Pull atmosphere into the container
                case AtmosphericVentMode.Intake:
                    if (AtmosRoom.Container.TryTransferTo(AtmosNetwork.Container, def, totalThroughput)) return true;

                    break;
                //Push atmosphere into the room
                case AtmosphericVentMode.Output:
                    if (def.networkValue == null) continue;
                    if (AtmosNetwork.Container.TryConsume(def.networkValue, totalThroughput))
                    {
                        ValueResult<AtmosphericDef> result;
                        if (def.dissipationGasDef != null)
                        {
                            parent.Map.GetMapInfo<AtmosphericMapInfo>().TrySpawnGasAt(parent.Position,
                                def.dissipationGasDef, totalThroughput * def.dissipationGasDef.maxDensityPerCell);
                            result = ValueResult<AtmosphericDef>.Init(totalThroughput, def).Complete(totalThroughput);
                        }
                        else
                        {
                            result = AtmosRoom.Container.TryAddValue(def, totalThroughput);
                        }

                        return result;
                    }

                    break;
                case AtmosphericVentMode.TwoWay:
                    TryEqualize(def, tick);
                    break;
            }

        return false;
    }

    //Helpers
    //Pressure check hack, if other container has higher room pressure it can send to this current vent
    private bool NeedsToReceiveFrom(Comp_AtmosphericNetworkStructure other)
    {
        return VentProps.ventMode switch
        {
            AtmosphericVentMode.Intake => false,
            AtmosphericVentMode.Output => true,
            AtmosphericVentMode.TwoWay => AtmosRoom.Container.StoredPercent < other.AtmosRoom.Container.StoredPercent,
            _ => false
        };
    }

    //Check whether we have vent neighbours that can receive
    private bool IsAtmosphericProvider()
    {
        var adjacencyList = AtmosNetwork.Network.Graph.GetAdjacencyList(AtmosNetwork);
        if (adjacencyList == null || !adjacencyList.Any()) return false;
        return adjacencyList.Any(c => c.Parent is Comp_ANS_VentBase pvent && pvent.NeedsToReceiveFrom(this));
    }
}