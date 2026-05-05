using TeleCore.Types;
using TeleCore.Types.Enums;
using TeleCore.Types.Exposables;

namespace TeleCore.FlowCore;

public class ClampWorker_QuarterLimit : ClampWorker
{
    public override string Description =>
        "Limit flow to a quarter of current content (outflow) or remaining space (inflow)";

    public override bool EnforceMinPipe => true;
    public override bool EnforceMaxPipe => true;
    public override bool MaintainFlowSpeed => false;
    public override float MinDivider => 4;
    public override float MaxDivider => 1;

    public override float ClampFunction(
        FlowInterface<NetworkPart, NetworkVolume, FlowVolumeConfig<>.Values.NetworkValueDef> iface, float f,
        ClampType type)
    {
        var t0 = iface.From;
        var t1 = iface.To;

        float c, r;
        if (EnforceMinPipe)
        {
            if (f > 0)
            {
                c = t0.TotalValue;
                f = ClampFlow(c, f, 0.25 * c);
            }
            else if (f < 0)
            {
                c = t1.TotalValue;
                f = -ClampFlow(c, -f, 0.25 * c);
            }
        }

        if (EnforceMaxPipe)
        {
            if (f > 0)
            {
                r = t1.MaxCapacity - t1.TotalValue;
                f = ClampFlow(r, f, 0.25 * r);
            }
            else if (f < 0)
            {
                r = t0.MaxCapacity - t0.TotalValue;
                f = -ClampFlow(r, -f, 0.25 * r);
            }
        }

        return f;
    }
}