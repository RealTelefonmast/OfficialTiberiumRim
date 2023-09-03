using UnityEngine;
using Verse;

namespace TR
{
    public class Graphic_SingleExtended : Graphic_Single
    {
        public override void DrawWorker(Vector3 loc, Rot4 rot, ThingDef thingDef, Thing thing, float extraRotation)
        {
            base.DrawWorker(loc, rot, thingDef, thing, extraRotation);
        }

        public override void Print(SectionLayer layer, Thing thing, float extraRotation)
        {
            base.Print(layer, thing, extraRotation);
        }
    }
}
