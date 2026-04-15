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
            TiberiumNetworkBuilding tnb = parent as TiberiumNetworkBuilding;
            if (tnb != null && tnb.ConnectableCells.Contains(c))
            {
                return true;
            }
            if (c.GetThingList(parent.Map).Any(b => tnb.StructureSet.FullList.Any(s => s == b)))
            {
                return true;
            }
            return false;
        }

        public override void DrawWorker(Vector3 loc, Rot4 rot, ThingDef thingDef, Thing parent, float extraRotation)
        {
            for (int i = 0; i < 4; i++)
            {
                IntVec3 cell = parent.Position + GenAdj.CardinalDirections[i];
                if (cell.InBounds(parent.Map))
                {
                    Graphics.DrawMesh(this.MeshAt(rot), cell.ToVector3ShiftedWithAltitude(parent.def.Altitude), Quaternion.identity, LinkedDrawMatFrom(parent, cell), 0);
                }
            }
        }

        public override void Print(SectionLayer layer, Thing parent)
        {
            base.Print(layer, parent);
            if (parent is TNW_Pipe pipe && !pipe.DirectParent.DestroyedOrNull())
            {
                Printer_Plane.PrintPlane(layer, pipe.ParentCell.ToVector3ShiftedWithAltitude(parent.def.Altitude), Vector2.one, LinkedDrawMatFrom(parent, pipe.ParentCell), 0f, false, null, null, 0.01f, 0f);
            }
        }
    }
}
