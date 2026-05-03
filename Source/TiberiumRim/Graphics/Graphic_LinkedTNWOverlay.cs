using System.Linq;
using UnityEngine;
using Verse;

namespace TiberiumRim;

public class Graphic_LinkedTNWOverlay : Graphic_Linked
{
    public Graphic_LinkedTNWOverlay()
    {
    }

    public Graphic_LinkedTNWOverlay(Graphic subGraphic) : base(subGraphic)
    {
        this.subGraphic = subGraphic;
    }

    public override bool ShouldLinkWith(IntVec3 c, Thing parent)
    {
        var tnb = parent as TiberiumNetworkBuilding;
        if (tnb != null && tnb.ConnectableCells.Contains(c)) return true;
        if (c.GetThingList(parent.Map).Any(b => tnb.StructureSet.FullList.Any(s => s == b))) return true;
        return false;
    }

    public bool ShouldLinkWith2(IntVec3 c, Thing parent)
    {
        var tnb = parent as TiberiumNetworkBuilding;
        return tnb != null && tnb.ConnectableCells.Contains(c);
    }

    public override void DrawWorker(Vector3 loc, Rot4 rot, ThingDef thingDef, Thing parent, float extraRotation)
    {
        Graphics.DrawMesh(MeshAt(rot), loc, Quaternion.identity, LinkedDrawMatFrom(parent, loc.ToIntVec3()), 0);
        for (var i = 0; i < 4; i++)
        {
            var cell = parent.Position + GenAdj.CardinalDirections[i];
            if (cell.InBounds(parent.Map) && ShouldLinkWith2(cell, parent))
                Graphics.DrawMesh(MeshAt(rot), cell.ToVector3Shifted(), Quaternion.identity,
                    LinkedDrawMatFrom(parent, cell), 0);
        }
    }

    public override void Print(SectionLayer layer, Thing thing)
    {
        foreach (var cell in (thing as TiberiumNetworkBuilding).ConnectableCells)
        {
            var vector = cell.ToVector3ShiftedWithAltitude(AltitudeLayer.MetaOverlays);
            Printer_Plane.PrintPlane(layer, vector, Vector2.one, base.LinkedDrawMatFrom(thing, cell));
        }
    }

    public Graphic_LinkedTNWOverlay ColoredVersion(Shader newShader, Color newColor, Color newColorTwo)
    {
        return new Graphic_LinkedTNWOverlay(subGraphic.GetColoredVersion(newShader, newColor, newColorTwo))
        {
            data = data
        };
    }
}