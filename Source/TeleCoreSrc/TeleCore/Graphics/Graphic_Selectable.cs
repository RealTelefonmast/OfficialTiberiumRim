using UnityEngine;
using Verse;

namespace TeleCore;

public class Graphic_Selectable : Graphic_Collection
{
    public override Material MatSingle { get; }

    public override Material MatSingleFor(Thing thing)
    {
        return base.MatSingleFor(thing);
    }

    public override Material MatAt(Rot4 rot, Thing thing = null)
    {
        return base.MatAt(rot, thing);
    }

    public Graphic SubGraphicFor(Thing thing)
    {
        if (thing == null)
            return subGraphics[0];
        var num = thing.overrideGraphicIndex ?? thing.thingIDNumber;
        return subGraphics[num % subGraphics.Length];
    }

    public override void DrawWorker(Vector3 loc, Rot4 rot, ThingDef thingDef, Thing thing, float extraRotation)
    {
        Graphic graphic;
        if (thing != null)
            graphic = SubGraphicFor(thing);
        else
            graphic = subGraphics[0];
        graphic.DrawWorker(loc, rot, thingDef, thing, extraRotation);
        if (ShadowGraphic != null)
            ShadowGraphic.DrawWorker(loc, rot, thingDef, thing, extraRotation);
    }

    public override void Print(SectionLayer layer, Thing thing, float extraRotation)
    {
        Graphic graphic;
        if (thing != null)
            graphic = SubGraphicFor(thing);
        else
            graphic = subGraphics[0];
        graphic.Print(layer, thing, extraRotation);
        if (ShadowGraphic != null && thing != null)
            ShadowGraphic.Print(layer, thing, extraRotation);
    }

    public Graphic AtIndex(int index)
    {
        if (index >= subGraphics.Length || index <= 0) return BaseContent.BadGraphic;
        return subGraphics[index];
    }
}