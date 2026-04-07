using TeleCore.FlowCore;
using TeleCore.Network.Flow.Values;

namespace TeleCore.Network.Flow.Clamping;

public class ClampWorker_QuarterLimit : ClampWorker
{
    public override string Description =>
        "Limit flow to a quarter of current content (outflow) or remaining space (inflow)";

    public override bool EnforceMinPipe => true;
    public override bool EnforceMaxPipe => true;
    public override bool MaintainFlowSpeed => false;
    public override double MinDivider => 4;
    public override double MaxDivider => 1;

    public override double ClampFunction(FlowInterface<NetworkPart, NetworkVolume, NetworkValueDef> iface, double f, ClampType type)
    {
        NetworkVolume t0 = iface.From;
        NetworkVolume t1 = iface.To;

        double c, r;
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