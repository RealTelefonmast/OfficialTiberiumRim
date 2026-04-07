using System;
using TeleCore.FlowCore;
using TeleCore.Network.Flow.Values;
using TeleCore.Primitive;

namespace TeleCore.Network.Flow.Pressure;

public abstract class PressureWorker
{
    public abstract string Description { get; }

    public abstract double CSquared { get; }
    public abstract double Friction { get; }


    public virtual DefValueStack<NetworkValueDef, double> FlowFunction(
        FlowInterface<NetworkPart, NetworkVolume, NetworkValueDef> iface, DefValueStack<NetworkValueDef, double> f)
    {
        throw new NotImplementedException();
    }
    
    [Obsolete]
    public abstract double FlowFunction(FlowInterface<NetworkPart, NetworkVolume, NetworkValueDef> iface, double f);

    public abstract double PressureFunction(NetworkVolume t);
    
    //TODO: Maybe move to utility class?
    public static double GetTotalFriction(NetworkVolume volume)
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

}