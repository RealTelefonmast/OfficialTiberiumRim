using RimWorld;
using TeleCore.Network.Flow;
using TeleCore.Network.Graph;
using TeleCore.Static;
using UnityEngine;
using Verse;

namespace TeleCore.Utils;

[StaticConstructorOnStartup]
internal static class DebugTools
{
    private static readonly Material FilledMat =
        SolidColorMaterials.NewSolidColorMaterial(Color.green, ShaderDatabase.MetaOverlay);

    private static readonly Material UnFilledMat =
        SolidColorMaterials.NewSolidColorMaterial(TColor.LightBlack, ShaderDatabase.MetaOverlay);

    #region Graph

    internal static void Debug_DrawGraphOnUI(NetworkGraph graph)
    {
        var size = Find.CameraDriver.CellSizePixels / 4;

        foreach (var (key, edge) in graph.UniqueEdges)
        {
            var thingA = edge.From.Thing;
            var thingB = edge.To.Thing;

            //TWidgets.DrawHalfArrow(ScreenPositionOf(thingA.TrueCenter()), ScreenPositionOf(thingB.TrueCenter()), Color.red, 8);

            //TODO: edge access only works for one version (node1, node2) - breaks two-way
            //TODO: some edges probably get setup broken (because only one edge is set)
            if (edge.IsValid)
            {
                TWidgets.DrawHalfArrow(edge.From.Parent.Thing.TrueCenter().ToScreenPos(),
                    edge.To.Parent.Thing.TrueCenter().ToScreenPos(), Color.red, size);
                if (edge.BiDirectional)
                    TWidgets.DrawHalfArrow(edge.To.Parent.Thing.TrueCenter().ToScreenPos(),
                    edge.From.Parent.Thing.TrueCenter().ToScreenPos(), Color.blue, size);
            }

            TWidgets.DrawBoxOnThing(thingA);
            TWidgets.DrawBoxOnThing(thingB);
        }
    }

    internal static void Debug_DrawPressure(NetworkFlowSystem networkFlowSys)
    {
        foreach (var (part, fb) in networkFlowSys.Relations)
        {
            var r = default(GenDraw.FillableBarRequest);
            r.center = part.Parent.Thing.Position.ToVector3() + new Vector3(0.25f, 0, 0.75f);
            r.size = new Vector2(1.5f, 0.5f);
            r.fillPercent = (float)fb.FillPercent;
            r.filledMat = FilledMat;
            r.unfilledMat = UnFilledMat;
            r.margin = 0f;
            r.rotation = Rot4.East;
            GenDraw.DrawFillableBar(r);
        }
    }

    internal static void Debug_DrawOverlays(NetworkGraph graph)
    {
        foreach (var node in graph.Nodes)
        {
            var pos = node.Value.Thing.DrawPos;
            GenMapUI.DrawText(new Vector2(pos.x, pos.z), $"[{node.Value.Thing}]", Color.green);
        }
    }

    #endregion
}