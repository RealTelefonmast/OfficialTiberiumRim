using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class Graphic_LinkedNetworkStructure : Graphic_Linked
    {
        public Graphic_LinkedNetworkStructure() { }

        public Graphic_LinkedNetworkStructure(Graphic subGraphic)
        {
            this.subGraphic = subGraphic;
        }

        public override bool ShouldLinkWith(IntVec3 c, Thing parent)
        {
            return c.InBounds(parent.Map) && parent.Map.Tiberium().NetworkInfo.HasConnectionAtFor(parent, c);
        }

        public override void Print(SectionLayer layer, Thing thing, float extraRotation)
        {
            var comp = thing.TryGetComp<Comp_NetworkStructure>();
            if (comp == null) return;

            //IntVec3 parentPos = thing.Position;
            //Printer_Plane.PrintPlane(layer, parentPos.ToVector3ShiftedWithAltitude(AltitudeLayer.FloorEmplacement), Vector2.one, LinkedDrawMatFrom(thing, parentPos));
            foreach (var pos in comp.InnerConnectionCells)
            {
                Printer_Plane.PrintPlane(layer, pos.ToVector3ShiftedWithAltitude(AltitudeLayer.FloorEmplacement), Vector2.one, LinkedDrawMatFrom(thing, pos));
            }
            //base.Print(layer, thing, extraRotation);
        }
    }
}
