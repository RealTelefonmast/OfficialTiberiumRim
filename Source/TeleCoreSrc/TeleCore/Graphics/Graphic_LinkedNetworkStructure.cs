using TeleCore.Comps;
using TeleCore.Unsorted;
using UnityEngine;
using Verse;

namespace TeleCore.Graphics;

public class Graphic_LinkedNetworkStructure : Graphic_Linked
{
    public Graphic_LinkedNetworkStructure()
    {
    }

    public Graphic_LinkedNetworkStructure(Graphic subGraphic)
    {
        this.subGraphic = subGraphic;
    }

    public override Material LinkedDrawMatFrom(Thing parent, IntVec3 cell)
    {
        return base.LinkedDrawMatFrom(parent, cell);
    }

    public override bool ShouldLinkWith(IntVec3 c, Thing parent)
    {
        return c.InBounds(parent.Map) && parent.Map.TeleCore().NetworkInfo.HasConnectionAtFor(parent, c);
    }

    public void Print(SectionLayer layer, Thing thing, float extraRotation, INetworkPart forPart)
    {
        foreach (var pos in forPart.PartIO.VisualCells)
            Printer_Plane.PrintPlane(layer, pos.Pos.Pos.ToVector3ShiftedWithAltitude(AltitudeLayer.FloorEmplacement),
                Vector2.one, LinkedDrawMatFrom(thing, pos));
    }

    public override void Print(SectionLayer layer, Thing thing, float extraRotation)
    {
        var comp = thing.TryGetComp<CompNetwork>();
        if (comp == null) return;

        foreach (var subPart in comp.NetworkParts)
        foreach (var pos in subPart.PartIO.VisualCells)
            Printer_Plane.PrintPlane(layer, pos.Pos.Pos.ToVector3ShiftedWithAltitude(AltitudeLayer.FloorEmplacement),
                Vector2.one, LinkedDrawMatFrom(thing, pos));
    }
}