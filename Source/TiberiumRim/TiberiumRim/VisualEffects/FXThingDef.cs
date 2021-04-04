using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class FXThingDef : ThingDef
    {
        public ExtendedGraphicData extraData = new ExtendedGraphicData();
        public GraphicData graphicData2;

        public override IEnumerable<string> ConfigErrors()
        {
            foreach (var error in base.ConfigErrors())
            {
                yield return error;
            }

            CompProperties_FX fxComp = this.GetCompProperties<CompProperties_FX>();
            if (fxComp != null)
            {
                foreach (var overlay in fxComp.overlays)
                {
                }
            }
        }
    }
}
