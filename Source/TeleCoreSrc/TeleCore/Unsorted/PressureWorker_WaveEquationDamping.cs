using TeleCore.Defs;

namespace TeleCore.Unsorted;

public class PressureWorker_WaveEquationDamping : PressureWorker
{
    public override string Description => "Model that can quickly eliminate waves without relying on friction.";

    public override double Friction => 0;
    public override double CSquared => 0.03;
    public double CSquaredDamper => 0.04;

    public override double FlowFunction(FlowInterface<NetworkPart, NetworkVolume, NetworkValueDef> iface, double f)
    {
        NetworkVolume from = iface.From;
        NetworkVolume to = iface.To;
        
        var dp = PressureFunction(from) - PressureFunction(to);
        var c = TMath.Sign(f) == TMath.Sign(dp) ? CSquared : CSquaredDamper;
        f += dp * c;
        f *= 1 - Friction;
        return f;
    }

    public override double PressureFunction(NetworkVolume t)
    {
        return t.TotalValue / t.MaxCapacity * 100;
    }
}