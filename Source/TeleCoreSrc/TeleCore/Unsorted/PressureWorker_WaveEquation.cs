using TeleCore.Defs;

namespace TeleCore.Unsorted;

public class PressureWorker_WaveEquation : PressureWorker
{
    public override string Description => "Wave equation with linear pressure.";

    public override double Friction => 0.001;
    public override double CSquared => 0.01;

    public override double FlowFunction(FlowInterface<NetworkPart, NetworkVolume, NetworkValueDef> iface, double f)
    {
        NetworkVolume from = iface.From;
        NetworkVolume to = iface.To;
        
        f += (PressureFunction(from) - PressureFunction(to)) * CSquared;
        f *= 1 - Friction;
        return f;
    }

    public override double PressureFunction(NetworkVolume t)
    {
        return t.TotalValue / t.MaxCapacity * 100;
    }
}