using System;
using TeleCore.Shared;

namespace TeleCore.FlowCore.Flow.Pressure;

public class PressureWorker_WaveEquationDamping3 : PressureWorker
{
    public override string Description => "Model that adds friction by making fluid stick to the pipe surface.";

    //TODO: Needs cleanup, technically only one pressureworker/flowfunc needed
    public override float Friction => 0.1f; //Unused 
    public override float CSquared => 0.03; //Unused

    public override DefValueStack<FlowVolumeConfig<>.Values.NetworkValueDef, float> FlowFunction(FlowInterface<NetworkPart, NetworkVolume, FlowVolumeConfig<>.Values.NetworkValueDef> iface, DefValueStack<FlowVolumeConfig<>.Values.NetworkValueDef, float> f)
    {
        var from = iface.From;
        var to = iface.To;
        var dp = PressureFunction(from) - PressureFunction(to);

        if (iface.Mode == InterfaceFlowMode.FromTo && dp <= 0)
        {
            return DefValueStack<FlowVolumeConfig<>.Values.NetworkValueDef, float>.Empty;
        }

        foreach (var valueDef in from.AllowedValues)
        {
            var fT = f[valueDef];
            var src = fT > 0 ? from : to;
            var srcPart = fT > 0 ? iface.FromPart : iface.ToPart;

            var contentDiff = Math.Abs((src.PrevStack.TotalValue - src.TotalValue).Value / src.MaxCapacity);
            if (from.Config.shareCapacity || to.Config.shareCapacity)
                dp = PressureFunction(from, valueDef) - PressureFunction(to, valueDef);

            fT += dp * srcPart.Config.CSquared;  //CSquared
            fT *= 1 - srcPart.Config.Friction; //Friction;
            fT *= 1 - GetTotalFriction(src);  //Additional Friction from each fluid/gas
            fT *= 1 - (0.5 * contentDiff);   //DampFriction
            fT *= iface.PassPercent;
            f[valueDef] = fT;
        }
        return f;
    }

    public override float FlowFunction(FlowInterface<NetworkPart, NetworkVolume, FlowVolumeConfig<>.Values.NetworkValueDef> iface, float f)
    {
        return 0;
        /*var from = iface.From;
        var to = iface.To;
        var dp = PressureFunction(from) - PressureFunction(to);

        if (iface.Mode == InterfaceFlowMode.FromTo && dp <= 0)
        {
            return 0;
        }
        
        var src = f > 0 ? from : to;
        var dc = Math.Max(0, src.PrevStack.TotalValue - src.TotalValue);
        f += dp * CSquared;
        f *= 1 - Friction;
        f *= 1 - GetTotalFriction(src); //Additional Friction from each fluid/gas
        f *= 1 - Math.Min(0.5, DampFriction * dc);
        return f;*/
    }

    public override float PressureFunction(NetworkVolume t)
    {
        return t.TotalValue / t.MaxCapacity * 100;
    }

    public float PressureFunction(NetworkVolume t, FlowVolumeConfig<>.Values.NetworkValueDef value)
    {
        return t.StoredValueOf(value) / t.CapacityPerType * 100;
    }
}