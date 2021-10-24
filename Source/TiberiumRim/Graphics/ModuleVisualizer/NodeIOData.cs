using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class NodeIOData
    {
        private ModuleNode parent;

        //I/O
        public NodeAnchorPanel InputPanel { get; private set; }
        public NodeAnchorPanel OutputPanel { get; private set; }

        public bool ConnectingOutput => OutputPanel.AnchorBeingPulled;

        public float Height { get; private set; }

        public NodeIOData(ModuleNode parent, int inputCount, int outputCount)
        {
            this.parent = parent;
            if (inputCount > 0)
                InputPanel = new NodeAnchorPanel(parent, inputCount, true);
            if (outputCount > 0)
                OutputPanel = new NodeAnchorPanel(parent, outputCount, false);

            Height = Math.Max(inputCount, outputCount) * ModuleVisualizer.AnchorHeight;
        }

        public bool HasInputAt(Vector2 toPos, out NodeAnchor inputAnchor)
        {
            inputAnchor = null;
            return InputPanel?.HasAnchorAt(toPos, out inputAnchor) ?? false;
        }

        public void DrawConnectionLine()
        {
            OutputPanel?.DrawConnectionLine();
        }

        public void DrawIO(Rect inRect)
        {
            InputPanel?.DrawInput(inRect.LeftHalf());
            OutputPanel?.DrawOutput(inRect.RightHalf());
        }

        public bool ConnectsTo(ModuleNode otherNode)
        {
            Log.Message($"Checking if {parent.ModuleName} connects to {otherNode?.ModuleName}");
            if (otherNode == null || OutputPanel == null || OutputPanel.AnchoredNodes == null) return false;
            Log.Message($"Checking all {OutputPanel.AnchoredNodes.Length} anchored nodes...");
            foreach (var anchor in OutputPanel.AnchoredNodes)
            {
                Log.Message($"Is {anchor} == {otherNode}? {anchor == otherNode}");
                if (anchor == otherNode)
                {
                    return true;
                }

                Log.Message($"Is {anchor} == {parent}? {anchor == parent}");
                if (anchor == parent) return false;
                if (anchor.IOAnchors.ConnectsTo(otherNode))
                {
                    return true;
                }
            }
            return false;
            //return OutputPanel?.AnchoredNodes.Any(n => n == otherNode || n.IOAnchors.ConnectsTo(otherNode)) ?? false;
        }
    }
}
