using System;

namespace TeleCore.FlowCore.Flow.Pressure;

public class PressureWorker_WaveEquationDamping2 : PressureWorker
{
    public override string Description => "Model that applies additional friction when waves occur.";

    public override float Friction => 0;
    public override float CSquared => 0.03;
    public float DampFriction => 0.01;

    public override float FlowFunction(FlowInterface<NetworkPart, NetworkVolume, FlowVolumeConfig<>.Values.NetworkValueDef> iface, float f)
    {
        NetworkVolume from = iface.From;
        NetworkVolume to = iface.To;

        var dp = PressureFunction(from) - PressureFunction(to);
        var counterFlow = Math.Sign(f) != Math.Sign(dp);
        f += dp * CSquared;
        f *= 1 - Friction;
        if (counterFlow) f *= 1 - Math.Min(0.9, DampFriction * Math.Abs(dp) * 0.01);
        return f;
    }

    public override float PressureFunction(NetworkVolume t)
    {
        return t.TotalValue / t.MaxCapacity * 100;
    }
}