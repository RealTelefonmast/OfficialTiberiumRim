namespace TeleCore.FlowCore.Flow.Pressure;

public class PressureWorker_FixedAcceleration : PressureWorker
{
    public override string Description =>
        "Fixed acceleration based on sign of pressure difference (as proposed by Quinor)";

    public override float CSquared { get; }
    public override float Friction { get; }

    public float Acceleration => 5;
    public float Inertia => 0.9;

    //
    public override float FlowFunction(FlowInterface<NetworkPart, NetworkVolume, FlowVolumeConfig<>.Values.NetworkValueDef> iface, float f)
    {
        NetworkVolume from = iface.From;
        NetworkVolume to = iface.To;

        f *= Inertia;
        f += (PressureFunction(from) - PressureFunction(to) > 0 ? 1 : -1) * Acceleration;
        return f;
    }

    public override float PressureFunction(NetworkVolume t)
    {
        return t.TotalValue / t.MaxCapacity * 100;
    }
}