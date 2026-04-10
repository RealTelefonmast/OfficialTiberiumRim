using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace TiberiumRim
{
    public class Graphic_LinkedTNW : Graphic_Linked
    {
        public Graphic_LinkedTNW() { }

        public Graphic_LinkedTNW(Graphic subGraphic)
        {
            this.subGraphic = subGraphic;
        }

        public override bool ShouldLinkWith(IntVec3 c, Thing parent)
        {
            return c.InBounds(parent.Map) && parent.Map.GetComponent<MapComponent_TNWManager>().ConnectionAt(c);
        }

        public override void Print(SectionLayer layer, Thing parent)
        {
            var comp = parent.TryGetComp<CompTNW>();
            if (comp == null) return;
            IntVec3 parentPos = parent.Position;
            Printer_Plane.PrintPlane(layer, parentPos.ToVector3ShiftedWithAltitude(AltitudeLayer.FloorEmplacement), Vector2.one, LinkedDrawMatFrom(parent, parentPos));
            foreach (var pos in comp.pipeExtensionCells)
            {
                Printer_Plane.PrintPlane(layer, pos.ToVector3ShiftedWithAltitude(AltitudeLayer.FloorEmplacement), Vector2.one, LinkedDrawMatFrom(parent, pos));
            }
        }
    }
}
