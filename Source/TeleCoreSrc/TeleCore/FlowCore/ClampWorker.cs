using System;
using TeleCore.Unsorted;

namespace TeleCore.FlowCore;

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
    public abstract float MinDivider { get; }

    /// <summary>
    ///     "Divider for remaining space [1..4]"
    /// </summary>
    public abstract float MaxDivider { get; }

    public virtual DefValueStack<FlowVolumeConfig<>.Values.NetworkValueDef, float> ClampFunction(
        FlowInterface<NetworkPart, NetworkVolume, FlowVolumeConfig<>.Values.NetworkValueDef> iface,
        DefValueStack<FlowVolumeConfig<>.Values.NetworkValueDef, float> f,
        ClampType type)
    {
        throw new NotImplementedException();
    }

    [Obsolete]
    public abstract float ClampFunction(
        FlowInterface<NetworkPart, NetworkVolume, FlowVolumeConfig<>.Values.NetworkValueDef> iface, float f,
        ClampType type);

    protected float ClampFlow(float content, float flow, float limit)
    {
        // 'content' can be available fluid or remaining space
        if (content <= 0)
            return 0;

        if (flow >= 0)
            return flow <= limit ? flow : limit;
        return flow >= -limit ? flow : -limit;
    }
}