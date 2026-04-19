using Verse;

namespace TR.Components;

public class MapComponent_TiberiumBiome : MapComponent
{
    private readonly MapComponent_Tiberium tiberium;

    public MapComponent_TiberiumBiome(Map map) : base(map)
    {
        tiberium = map.GetComponent<MapComponent_Tiberium>();
    }

    public TibWeatherLevel WeatherLevel
    {
        get
        {
            var value = tiberium.TiberiumInfo.Coverage;
            if (value >= 0.45f)
                return TibWeatherLevel.Prefer;
            if (value >= 0.8f)
                return TibWeatherLevel.Suppress;
            return TibWeatherLevel.Allow;
        }
    }

    public void DecideWeather()
    {
    }
}

public enum TibWeatherLevel
{
    Allow,
    Prefer,
    Suppress
}