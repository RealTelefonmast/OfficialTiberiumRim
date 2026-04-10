using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;

namespace TiberiumRim
{
    public class Graphic_SingleExtended : Graphic_Single
    {
        public override void DrawWorker(Vector3 loc, Rot4 rot, ThingDef thingDef, Thing thing, float extraRotation)
        {
            base.DrawWorker(loc, rot, thingDef, thing, extraRotation);
        }

        public override void Print(SectionLayer layer, Thing thing)
        {
            base.Print(layer, thing);
        }
    }
}
