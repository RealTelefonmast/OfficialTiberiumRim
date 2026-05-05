using TeleCore.Defs;
using TeleCore.Types;
using TeleCore.Types.Exposables;
using TeleCore.Types.Utils;

namespace TeleCore.FlowCore;

public class PressureWorker_WaveEquationDamping2 : PressureWorker
{
    public override string Description => "Model that applies additional friction when waves occur.";

    public override double Friction => 0;
    public override double CSquared => 0.03;
    public double DampFriction => 0.01;

    public override double FlowFunction(FlowInterface<NetworkPart, NetworkVolume, NetworkValueDef> iface, double f)
    {
        var from = iface.From;
        var to = iface.To;

        var dp = PressureFunction(from) - PressureFunction(to);
        var counterFlow = TMath.Sign(f) != TMath.Sign(dp);
        f += dp * CSquared;
        f *= 1 - Friction;
        if (counterFlow) f *= 1 - TMath.Min(0.9, DampFriction * TMath.Abs(dp) * 0.01);
        return f;
    }

    public override double PressureFunction(NetworkVolume t)
    {
        return t.TotalValue / t.MaxCapacity * 100;
    }
}