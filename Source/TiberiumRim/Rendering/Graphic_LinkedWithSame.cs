using UnityEngine;
using Verse;
using RimWorld;

namespace TiberiumRim
{
    public class Graphic_LinkedWithSame : Graphic_Linked
    {
        public override Material MatSingle
        {
            get
            {
                return MaterialAtlasPool.SubMaterialFromAtlas(subGraphic.MatSingle, LinkDirections.None);
            }
        }

        public Graphic_LinkedWithSame() { }

        public Graphic_LinkedWithSame(Graphic subGraphic)
        {
            this.subGraphic = subGraphic;
        }

        public override Graphic GetColoredVersion(Shader newShader, Color newColor, Color newColorTwo)
        {
            return new Graphic_LinkedWithSame(this.subGraphic.GetColoredVersion(newShader, newColor, newColorTwo))
            {
                data = this.data
            };
        }

        public override void Init(GraphicRequest req)
        {
            subGraphic = GraphicDatabase.Get<Graphic_Single>(req.path, req.shader, Vector2.one, Color.white, Color.white, data);
        }

        public override bool ShouldLinkWith(IntVec3 c, Thing parent)
        {
            var sameThing = c.GetFirstThing(parent.Map, parent.def);
            return c.InBounds(parent.Map) && sameThing != null;
        }

        public override void DrawWorker(Vector3 loc, Rot4 rot, ThingDef thingDef, Thing thing, float extraRotation)
        {
            base.DrawWorker(loc, rot, thingDef, thing, extraRotation);
        }

        public override void Print(SectionLayer layer, Thing thing, float extraRotation)
        {
            Material mat = LinkedDrawMatFrom(thing, thing.Position);
            Printer_Plane.PrintPlane(layer, thing.TrueCenter(), new Vector2(1f, 1f), mat, extraRotation, false, null, null, 0.01f, 0f);
        }
    }
}
