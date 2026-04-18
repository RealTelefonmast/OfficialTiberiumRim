using TeleCore.UI;
using UnityEngine;
using Verse;

namespace TeleCore.Unsorted;

public class NodeAnchor
{
    //Render Data
    private Rect anchorRect;

    private Vector2? currentLineEnd;
    private readonly int index;

    private readonly bool isInput;
    private readonly NodeAnchorPanel parentPanel;
    private NodeAnchor targetAnchor;

    public NodeAnchor(NodeAnchorPanel parent, int index, bool isInput)
    {
        parentPanel = parent;
        this.index = index;
        this.isInput = isInput;
    }

    public ModuleNode ParentNode => parentPanel.Parent;
    public ModuleNode TargetNode => targetAnchor.ParentNode;

    public bool HasTarget => targetAnchor != null;

    public bool AnchorPulled { get; private set; }

    private Vector2 Size => new(Window_ModuleVisualizer.AnchorHeight, Window_ModuleVisualizer.AnchorHeight);

    public Rect AnchorRect => anchorRect;
    public Vector2 Position { get; private set; }

    public Vector2 CenterPos => Position + Size / 2;

    public void ConnectTo(NodeAnchor other)
    {
        targetAnchor?.Disconnect();
        targetAnchor = other;

        if (isInput) ParentNode.Notify_NewInput(index, targetAnchor.ParentNode);
    }

    private void Disconnect()
    {
        targetAnchor = null;
    }

    public void DrawConnectionLine()
    {
        if (isInput) return;
        if (targetAnchor != null) Verse.Widgets.DrawLine(CenterPos, targetAnchor.CenterPos, Color.red, 4);

        if (currentLineEnd != null) Verse.Widgets.DrawLine(CenterPos, currentLineEnd.Value, Color.cyan, 4);
    }

    public void DrawAt(Vector2 pos)
    {
        Position = pos;
        anchorRect = new Rect(Position.x, Position.y, Size.x, Size.y);

        //
        DoAnchorControl();

        //
        var text = targetAnchor != null
            ? isInput ? TeleContent.NodeIn_Closed : TeleContent.NodeOut_Closed
            : TeleContent.Node_Open;
        Verse.Widgets.DrawTextureFitted(anchorRect, text, 1);
    }

    private void DoAnchorControl()
    {
        var curEv = Event.current;
        var evType = curEv.type;
        if (Mouse.IsOver(anchorRect))
            if (evType == EventType.MouseDown)
            {
                //Start connecting
                AnchorPulled = true;
                Window_ModuleVisualizer.Vis.MakingNewConnection = true;
            }

        if (evType == EventType.MouseDrag && AnchorPulled)
            //Update line
            currentLineEnd = Event.current.mousePosition;

        if (evType == EventType.MouseUp && curEv.button == 0)
        {
            //Stop connecting
            if (currentLineEnd == null) return;
            parentPanel.Notify_TryConnectAt(this, currentLineEnd.GetValueOrDefault());

            //Reset
            currentLineEnd = null;
            AnchorPulled = false;
            Window_ModuleVisualizer.Vis.MakingNewConnection = false;
        }
    }

    internal bool RectContains(Vector2 toPos)
    {
        return anchorRect.Contains(toPos);
    }
}