namespace TeleCore.FlowCore.Flow.Pressure;

public class PressureWorker_WaveEquationNonLinear : PressureWorker
{
    public override string Description => "Wave equation with non-linear pressure.";

    public override float Friction => 0.001;
    public override float CSquared => 0.01;

    public override float FlowFunction(FlowInterface<NetworkPart, NetworkVolume, FlowVolumeConfig<>.Values.NetworkValueDef> iface, float f)
    {
        NetworkVolume from = iface.From;
        NetworkVolume to = iface.To;

        f += (PressureFunction(from) - PressureFunction(to)) * CSquared;
        f *= 1 - Friction;
        return f;
    }

    public override float PressureFunction(NetworkVolume t)
    {
        var p = t.TotalValue / t.MaxCapacity * 100;
        return p <= 60 ? p : 60 + (p - 60) * 10;
    }
}