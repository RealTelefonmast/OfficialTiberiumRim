using System.Xml;
using RimWorld;

namespace TeleCore.Types.Structs;

public struct TickTime
{
    public float Hours => TotalTicks / (float)GenDate.TicksPerHour;
    public float Days => TotalTicks / (float)GenDate.TicksPerDay;

    public int TotalTicks { get; private set; }

    public TickTime(int ticks)
    {
        TotalTicks = ticks;
    }

    private void LoadDataFromXmlCustom(XmlNode xmlRoot)
    {
        var value = xmlRoot.Value;

        if (value.EndsWith("h"))
            TotalTicks = (int)float.Parse(value.Substring(0, value.Length - 1)) * GenDate.TicksPerHour;
        else if (value.EndsWith("d"))
            TotalTicks = (int)float.Parse(value.Substring(0, value.Length - 1)) * GenDate.TicksPerDay;

        if (int.TryParse(value, out var ticksVal)) TotalTicks = ticksVal;
    }

    public override string ToString()
    {
        return TotalTicks.ToStringTicksToPeriodVerbose(true, false);
    }
}