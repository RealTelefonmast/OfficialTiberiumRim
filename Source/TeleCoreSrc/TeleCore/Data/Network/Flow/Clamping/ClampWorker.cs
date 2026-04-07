using System;
using TeleCore.FlowCore;
using TeleCore.Network.Flow.Values;
using TeleCore.Primitive;

namespace TeleCore.Network.Flow.Clamping;

public abstract class ClampWorker
{
    public abstract string Description { get; }

    /// <summary>
    ///     "Enforce pipe min content (= 0)"
    /// </summary>
    public abstract bool EnforceMinPipe { get; }

    /// <summary>
    ///     "Enforce pipe max content (= 100)"
    /// </summary>
    public abstract bool EnforceMaxPipe { get; }

    /// <summary>
    ///     "Do not reduce flow speed when clamping"
    /// </summary>
    public abstract bool MaintainFlowSpeed { get; }

    /// <summary>
    ///     "Divider for available fluid [1..4]"
    /// </summary>
    public abstract double MinDivider { get; }

    /// <summary>
    ///     "Divider for remaining space [1..4]"
    /// </summary>
    public abstract double MaxDivider { get; }

    public virtual DefValueStack<NetworkValueDef, double> ClampFunction(
        FlowInterface<NetworkPart, NetworkVolume, NetworkValueDef> iface, DefValueStack<NetworkValueDef, double> f,
        ClampType type)
    {
        throw new NotImplementedException();
    }

    [Obsolete]
    public abstract double ClampFunction(FlowInterface<NetworkPart, NetworkVolume, NetworkValueDef> iface, double f, ClampType type);

    protected double ClampFlow(double content, double flow, double limit)
    {
        // 'content' can be available fluid or remaining space
        if (content <= 0) 
            return 0;

        if (flow >= 0) 
            return flow <= limit ? flow : limit;
        return flow >= -limit ? flow : -limit;
    }
}