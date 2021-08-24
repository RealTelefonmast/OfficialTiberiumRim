using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class Graphic_Linked_NetworkStructureOverlay : Graphic_LinkedNetworkStructure
    {
        public Graphic_Linked_NetworkStructureOverlay(){ }

        public Graphic_Linked_NetworkStructureOverlay(Graphic subGraphic) : base(subGraphic)
        {
            this.subGraphic = subGraphic;
        }

        public override bool ShouldLinkWith(IntVec3 c, Thing parent)
        {
            return base.ShouldLinkWith(c, parent);
        }

        public override void DrawWorker(Vector3 loc, Rot4 rot, ThingDef thingDef, Thing parent, float extraRotation)
        {
            Graphics.DrawMesh(this.MeshAt(rot), loc, Quaternion.identity, LinkedDrawMatFrom(parent, loc.ToIntVec3()), 0);
            for (int i = 0; i < 4; i++)
            {
                IntVec3 cell = parent.Position + GenAdj.CardinalDirections[i];
                if (cell.InBounds(parent.Map) && ShouldLinkWith(cell, parent))
                {
                    Graphics.DrawMesh(this.MeshAt(rot), cell.ToVector3Shifted(), Quaternion.identity, LinkedDrawMatFrom(parent, cell), 0);
                }
            }
        }

        public override void Print(SectionLayer layer, Thing parent, float extraRotation)
        {
            var comp = parent.TryGetComp<Comp_NetworkStructure>();
            foreach (IntVec3 cell in comp.InnerConnectionCells)
            {
                Vector3 center = cell.ToVector3ShiftedWithAltitude(AltitudeLayer.MetaOverlays);
                Printer_Plane.PrintPlane(layer, center, new Vector2(1f, 1f), LinkedDrawMatFrom(parent, cell), extraRotation, false, null, null, 0.01f, 0f);
            }
        }

        public Graphic_Linked_NetworkStructureOverlay ColoredVersion(Shader newShader, Color newColor, Color newColorTwo)
        {
            return new Graphic_Linked_NetworkStructureOverlay(this.subGraphic.GetColoredVersion(newShader, newColor, newColorTwo))
            {
                data = this.data
            };
        }
    }
}
