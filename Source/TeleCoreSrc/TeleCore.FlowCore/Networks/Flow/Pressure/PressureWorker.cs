using System;
using TeleCore.Shared;

namespace TeleCore.FlowCore.Flow.Pressure;

public abstract class PressureWorker
{
    public abstract string Description { get; }

    public abstract float CSquared { get; }
    public abstract float Friction { get; }


    public virtual DefValueStack<FlowVolumeConfig<>.Values.NetworkValueDef, float> FlowFunction(
        FlowInterface<NetworkPart, NetworkVolume, FlowVolumeConfig<>.Values.NetworkValueDef> iface, DefValueStack<FlowVolumeConfig<>.Values.NetworkValueDef, float> f)
    {
        throw new NotImplementedException();
    }

    [Obsolete]
    public abstract float FlowFunction(FlowInterface<NetworkPart, NetworkVolume, FlowVolumeConfig<>.Values.NetworkValueDef> iface, float f);

    public abstract float PressureFunction(NetworkVolume t);

    //TODO: Maybe move to utility class?
    public static float GetTotalFriction(NetworkVolume volume)
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

        var averageFriction = totalFriction / totalVolume;
        return averageFriction;
    }

}