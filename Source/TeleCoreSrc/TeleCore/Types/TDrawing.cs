using TeleCore.Defs;
using TeleCore.Visual.VFX.FX.Layer;
using UnityEngine;
using Verse;

namespace TeleCore.Types;

public class TDrawing
{
    public static void Draw(Graphic graphic, Vector3 drawPos, Rot4 rot, float? rotation, ThingDef def,
        Thing thing = null, FXDefExtension extension = null)
    {
        FXLayer.GetDrawInfo(graphic, thing, def, ref drawPos, rot, extension, out _, out var drawMat, out var drawMesh,
            out var exactRotation, out _);
        Graphics.DrawMesh(drawMesh, drawPos, rotation?.ToQuat() ?? exactRotation.ToQuat(), drawMat, 0);
    }

    public static void PrintBasic(SectionLayer layer, Vector3 drawPos, Vector2 drawSize, Material drawMat,
        float exactRotation, bool flipUV)
    {
        Printer_Plane.PrintPlane(layer, drawPos, drawSize, drawMat, exactRotation, flipUV);
    }

    public static void Print(SectionLayer layer, Graphic graphic, ThingWithComps thing, ThingDef def,
        FXDefExtension extension = null)
    {
        if (graphic is Graphic_Linked or Graphic_Appearances)
        {
            graphic.Print(layer, thing, 0);
            return;
        }

        if (graphic is Graphic_Random rand)
            graphic = rand.SubGraphicFor(thing);
        var drawPos = thing.DrawPos;
        FXLayer.GetDrawInfo(graphic, thing, def, ref drawPos, thing.Rotation, extension, out var drawSize,
            out var drawMat, out _, out var exactRotation, out var flipUV);
        Printer_Plane.PrintPlane(layer, drawPos, drawSize, drawMat, exactRotation, flipUV);
        if (graphic.ShadowGraphic != null && thing != null) graphic.ShadowGraphic.Print(layer, thing, exactRotation);
        thing.AllComps.ForEach(c => c.PostPrintOnto(layer));
    }
}