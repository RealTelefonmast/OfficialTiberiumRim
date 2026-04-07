// Preserved from TeleCore/Caching/AtmosphericCache.cs (old stub)
using System.Collections.Generic;
using TeleCore;
using UnityEngine;
using Verse;

namespace TeleCore.Atmosphere.OldRef
{
    /// <summary>
    /// Wraps the scribed Atmospheric data into an object for data encapsulation in XML.
    /// </summary>
    public class AtmosphericCache_TAE : IExposable
    {
        private AtmosphericScriber_TAE _scriber;

        public TAE.AtmosphericMapInfo AtmosphericMapInfo => map.GetMapInfo<TAE.AtmosphericMapInfo>();

        private Map map;
        private AtmosphericScriber_TAE scriber;

        public AtmosphericCache_TAE(Map map)
        {
            this.map = map;
            scriber = new AtmosphericScriber_TAE(map);
        }

        public void ExposeData()
        {
            scriber.ScribeData();
        }
    }
}
