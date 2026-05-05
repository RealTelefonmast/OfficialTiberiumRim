using TeleCore.Unsorted;

namespace TeleCore.FlowCore;

public class ClampWorker_Overcommit : ClampWorker
{
    public override string Description =>
        "Limit flow to a configurable fraction of current content (outflow) or remaining space (inflow)";

    public override bool EnforceMinPipe => true;
    public override bool EnforceMaxPipe => true;
    public override bool MaintainFlowSpeed => false;
    public override float MinDivider => 4;
    public override float MaxDivider => 1;

    public override DefValueStack<FlowVolumeConfig<>.Values.NetworkValueDef, float> ClampFunction(
        FlowInterface<NetworkPart, NetworkVolume, FlowVolumeConfig<>.Values.NetworkValueDef> iface,
        DefValueStack<FlowVolumeConfig<>.Values.NetworkValueDef, float> f, ClampType type)
    {
        var from = iface.From;
        var to = iface.To;

        float d, c, r;
        var fTotal = f.TotalValue.Value;
        if (fTotal == 0) return f;

        if (EnforceMinPipe)
        {
            // Limit outflow to 1/divider of fluid content in src pipe     
            if (type == ClampType.FlowSpeed && MaintainFlowSpeed)
                d = 1;
            else
                d = 1 / MinDivider;
            if (fTotal > 0)
            {
                c = from.TotalValue;
                fTotal = ClampFlow(c, fTotal, d * c);
            }
            else if (f < 0)
            {
                c = to.TotalValue;
                fTotal = -ClampFlow(c, -fTotal, d * c);
            }
        }

        if (EnforceMaxPipe && (type == ClampType.FluidMove || !MaintainFlowSpeed))
        {
            // Limit inflow to 1/divider of remaining space in dst pipe
            d = 1 / MaxDivider;
            if (fTotal > 0)
            {
                r = to.MaxCapacity - to.TotalValue;
                fTotal = ClampFlow(r, fTotal, d * r);
            }
            else if (f < 0)
            {
                r = from.MaxCapacity - from.TotalValue;
                fTotal = -ClampFlow(r, -fTotal, d * r);
            }
        }

        return f * (fTotal / f.TotalValue.Value);
    }

    public override float ClampFunction(
        FlowInterface<NetworkPart, NetworkVolume, FlowVolumeConfig<>.Values.NetworkValueDef> iface, float f,
        ClampType type)
    {
        var t0 = iface.From;
        var t1 = iface.To;

        float d, c, r;
        if (EnforceMinPipe)
        {
            // Limit outflow to 1/divider of fluid content in src pipe     
            if (type == ClampType.FlowSpeed && MaintainFlowSpeed)
                d = 1;
            else
                d = 1 / MinDivider;
            if (f > 0)
            {
                c = t0.TotalValue;
                f = ClampFlow(c, f, d * c);
            }
            else if (f < 0)
            {
                c = t1.TotalValue;
                f = -ClampFlow(c, -f, d * c);
            }
        }

        if (EnforceMaxPipe && (type == ClampType.FluidMove || !MaintainFlowSpeed))
        {
            // Limit inflow to 1/divider of remaining space in dst pipe
            d = 1 / MaxDivider;
            if (f > 0)
            {
                r = t1.MaxCapacity - t1.TotalValue;
                f = ClampFlow(r, f, d * r);
            }
            else if (f < 0)
            {
                r = t0.MaxCapacity - t0.TotalValue;
                f = -ClampFlow(r, -f, d * r);
            }
        }

        return f;
    }
}