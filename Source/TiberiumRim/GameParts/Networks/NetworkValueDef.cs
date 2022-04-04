using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class NetworkValueDef : Def
    {
        public string labelShort;
        public NetworkDef networkDef;
        public Color valueColor;
        public List<ThingDefCountClass> valueThings;

        public override void ResolveReferences()
        {
            base.ResolveReferences();
            if (labelShort.NullOrEmpty())
            {
                labelShort = label;
            }

            networkDef.ResolvedValueDef(this);
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
