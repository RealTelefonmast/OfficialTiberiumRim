using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace TiberiumRim.Weather
{
    public class MapComponent_TiberiumBiome : MapComponent
    {

        private MapComponent_Tiberium tiberium;
        public MapComponent_TiberiumBiome(Map map) : base(map)
        {
            tiberium = map.GetComponent<MapComponent_Tiberium>();
        }

        public void DecideWeather()
        {

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
    }

    public enum TibWeatherLevel
    {
        Allow,
        Prefer,
        Suppress
    }
}
