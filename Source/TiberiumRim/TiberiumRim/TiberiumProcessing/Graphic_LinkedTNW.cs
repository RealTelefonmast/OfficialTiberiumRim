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
            if(comp != null)
            {
                if (comp is CompTNW_Pipe)
                {
                    base.Print(layer, parent);
                }
                for(int i = 0; i < comp.pipeExtensionCells.Count; i++)
                {
                    IntVec3 pos = comp.pipeExtensionCells[i];
                    Printer_Plane.PrintPlane(layer, pos.ToVector3ShiftedWithAltitude(parent.def.Altitude + (comp is CompTNW_Pipe ? 0 : -1)), Vector2.one, LinkedDrawMatFrom(parent, pos));
                }
            }
        }
    }
}
