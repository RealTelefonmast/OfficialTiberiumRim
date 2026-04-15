// Preserved from TeleCore/Caching/AtmosphericCache.cs (old stub)
using TeleCore.Utils;
using Verse;

namespace TeleCore.Atmosphere.OldRef;

/// <summary>
///     Wraps the scribed Atmospheric data into an object for data encapsulation in XML.
/// </summary>
public class AtmosphericCache_TAE : IExposable
{
    private readonly Map map;
    private readonly AtmosphericScriber_TAE scriber;

    public AtmosphericCache_TAE(Map map)
    {
        this.map = map;
        scriber = new AtmosphericScriber_TAE(map);
    }

    public AtmosphericMapInfo AtmosphericMapInfo => map.GetMapInfo<AtmosphericMapInfo>();

    public void ExposeData()
    {
        scriber.ScribeData();
    }
}
