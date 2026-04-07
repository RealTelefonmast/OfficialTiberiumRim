using TeleCore.Defs;
using TeleCore.ThingComps;
using UnityEngine;
using Verse;

namespace TeleCore.Graphics;

public class Graphic_Linked_NetworkStructureOverlay : Graphic_LinkedNetworkStructure
{
    public Graphic_Linked_NetworkStructureOverlay()
    {
    }

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
        UnityEngine.Graphics.DrawMesh(MeshAt(rot), loc, Quaternion.identity, LinkedDrawMatFrom(parent, loc.ToIntVec3()), 0);
        for (var i = 0; i < 4; i++)
        {
            var cell = parent.Position + GenAdj.CardinalDirections[i];
            if (cell.InBounds(parent.Map) && ShouldLinkWith(cell, parent))
                UnityEngine.Graphics.DrawMesh(MeshAt(rot), cell.ToVector3Shifted(), Quaternion.identity,
                    LinkedDrawMatFrom(parent, cell), 0);
        }
    }

    public void Print(SectionLayer layer, Thing parent, float extraRotation, NetworkDef forNetwork)
    {
        var comp = parent.TryGetComp<CompNetwork>();
        foreach (IntVec3 cell in comp[forNetwork].PartIO.VisualCells)
        {
            var center = cell.ToVector3ShiftedWithAltitude(AltitudeLayer.MetaOverlays);
            Printer_Plane.PrintPlane(layer, center, new Vector2(1f, 1f), LinkedDrawMatFrom(parent, cell),
                extraRotation);
        }
    }

    public override void Print(SectionLayer layer, Thing parent, float extraRotation)
    {
        Print(layer, parent, extraRotation, SectionLayer_NetworkGrid.CURRENT_NETWORK);
    }

    public Graphic_Linked_NetworkStructureOverlay ColoredVersion(Shader newShader, Color newColor, Color newColorTwo)
    {
        return new Graphic_Linked_NetworkStructureOverlay(
            subGraphic.GetColoredVersion(newShader, newColor, newColorTwo))
        {
            data = data
        };
    }
}