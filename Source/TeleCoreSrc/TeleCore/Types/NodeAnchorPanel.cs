using System.Linq;
using TeleCore.UI;
using UnityEngine;
using Verse;

namespace TeleCore.Unsorted;

public class NodeAnchorPanel
{
    private bool isInput;

    public NodeAnchorPanel(ModuleNode parent, int anchorCount, bool isInputPanel)
    {
        Parent = parent;
        isInput = isInputPanel;

        Anchors = new NodeAnchor[anchorCount]; //isInputPanel ? new NodeAnchor[parent.ModuleData.Inputs.Length] : new NodeAnchor[1];
        for (var i = 0; i < anchorCount; i++) Anchors[i] = new NodeAnchor(this, i, isInputPanel);
    }

    public ModuleNode[] AnchoredNodes => HasAnchoredNode ? Anchors.Select(t => t.TargetNode).ToArray() : null;
    public NodeAnchor[] Anchors { get; }

    public bool HasAnchoredNode => Anchors.Any(n => n.HasTarget);
    public bool AnchorBeingPulled => Anchors[0].AnchorPulled;
    public ModuleNode Parent { get; }

    private int AnchorCount => Anchors?.Length ?? 0;

    public bool HasAnchorAt(Vector2 toPos, out NodeAnchor inputAnchor)
    {
        inputAnchor = Anchors.FirstOrFallback(a => a.RectContains(toPos));
        return inputAnchor != null;
    }

    public void DrawConnectionLine()
    {
        foreach (var anchor in Anchors) anchor.DrawConnectionLine();
    }

    public void DrawInput(Rect inRect)
    {
        Widgets.DrawBoxSolid(inRect, TColor.BGLighter);
        var pos = new Vector2(inRect.x, inRect.y);
        float curY = 0;
        foreach (var anchor in Anchors)
        {
            anchor.DrawAt(new Vector2(pos.x, pos.y + curY));
            curY += Window_ModuleVisualizer.AnchorHeight;
        }
    }

    public void DrawOutput(Rect inRect)
    {
        Widgets.DrawBoxSolid(inRect, TColor.BGDarker);
        var pos = new Vector2(inRect.x + (inRect.width - Window_ModuleVisualizer.AnchorHeight), inRect.y);
        Anchors[0].DrawAt(pos);
    }

    public void Notify_TryConnectAt(NodeAnchor fromAnchor, Vector2 currentLineEnd)
    {
        Parent.Notify_TryConnectAt(fromAnchor, currentLineEnd);
    }
}