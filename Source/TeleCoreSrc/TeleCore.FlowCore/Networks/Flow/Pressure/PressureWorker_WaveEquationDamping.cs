using System;

namespace TeleCore.FlowCore.Flow.Pressure;

public class PressureWorker_WaveEquationDamping : PressureWorker
{
    public override string Description => "Model that can quickly eliminate waves without relying on friction.";

    public override float Friction => 0;
    public override float CSquared => 0.03;
    public float CSquaredDamper => 0.04;

    public override float FlowFunction(FlowInterface<NetworkPart, NetworkVolume, FlowVolumeConfig<>.Values.NetworkValueDef> iface, float f)
    {
        NetworkVolume from = iface.From;
        NetworkVolume to = iface.To;

        var dp = PressureFunction(from) - PressureFunction(to);
        var c = Math.Sign(f) == Math.Sign(dp) ? CSquared : CSquaredDamper;
        f += dp * c;
        f *= 1 - Friction;
        return f;
    }

    public override float PressureFunction(NetworkVolume t)
    {
        return t.TotalValue / t.MaxCapacity * 100;
    }
}