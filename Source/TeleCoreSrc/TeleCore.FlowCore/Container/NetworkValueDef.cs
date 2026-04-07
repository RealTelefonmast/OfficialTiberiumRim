using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace TeleCore
{
    public class NetworkValueDef : Def
    {
        //
        public string labelShort;
        public string valueUnit;
        public Color valueColor;
        public NetworkDef networkDef;

        //
        public ThingDef thingDroppedFromContainer;
        public float valueToThingRatio = 1;

        public override void ResolveReferences()
        {
            base.ResolveReferences();
            if (labelShort.NullOrEmpty())
            {
                labelShort = label;
            }

            networkDef.Notify_ResolvedNetworkValueDef(this);
        }

        public override IEnumerable<string> ConfigErrors()
        {
            foreach (var configError in base.ConfigErrors())
            {
                yield return configError;
            }
        }
    }
}
