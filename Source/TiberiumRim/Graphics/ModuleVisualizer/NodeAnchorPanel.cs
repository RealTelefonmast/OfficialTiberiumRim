using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class NodeAnchorPanel
    {
        private ModuleNode parent;
        private NodeAnchor[] anchors;

        private bool isInput;

        public ModuleNode[] AnchoredNodes => HasAnchoredNode ? anchors.Select(t => t.TargetNode).ToArray() : null;
        public NodeAnchor[] Anchors => anchors;

        public bool HasAnchoredNode => anchors.Any(n => n.HasTarget);
        public bool AnchorBeingPulled => anchors[0].AnchorPulled;
        public ModuleNode Parent => parent;

        private int AnchorCount => anchors?.Length ?? 0;

        public NodeAnchorPanel(ModuleNode parent, int anchorCount, bool isInputPanel)
        {
            this.parent = parent;
            isInput = isInputPanel;

            this.anchors = new NodeAnchor[anchorCount];//isInputPanel ? new NodeAnchor[parent.ModuleData.Inputs.Length] : new NodeAnchor[1];
            for (int i = 0; i < anchorCount; i++)
            {
                this.anchors[i] = new NodeAnchor(this,i, isInputPanel);
            }
        }

        public bool HasAnchorAt(Vector2 toPos, out NodeAnchor inputAnchor)
        {
            inputAnchor = anchors.FirstOrFallback(a => a.RectContains(toPos));
            return inputAnchor != null;
        }

        public void DrawConnectionLine()
        {
            foreach (var anchor in anchors)
            {
                anchor.DrawConnectionLine();
            }
        }

        public void DrawInput(Rect inRect)
        {
            Widgets.DrawBoxSolid(inRect, TRMats.BGLighter);
            var pos = new Vector2(inRect.x, inRect.y);
            float curY = 0;
            foreach (var anchor in anchors)
            {
                anchor.DrawAt(new Vector2(pos.x, pos.y + curY));
                curY += ModuleVisualizer.AnchorHeight;
            }
        }

        public void DrawOutput(Rect inRect)
        {
            Widgets.DrawBoxSolid(inRect, TRMats.BGDarker);
            var pos = new Vector2(inRect.x + (inRect.width - ModuleVisualizer.AnchorHeight), inRect.y);
            anchors[0].DrawAt(pos);
        }

        public void Notify_TryConnectAt(NodeAnchor fromAnchor, Vector2 currentLineEnd)
        {
            parent.Notify_TryConnectAt(fromAnchor, currentLineEnd);
        }
    }
}
