using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace TiberiumRim
{
    public abstract class ModuleNode
    {
        //Data
        private ModuleData moduleData;
        private NodeRenderData renderData;
        private NodeIOData IOData;

        public ModuleData ModuleData => moduleData;
        public NodeRenderData Renderer => renderData;
        public NodeIOData IOAnchors => IOData;

        protected abstract int Inputs { get; }
        protected abstract int Outputs { get; }

        //Graphical
        public virtual Vector2 NodeSize => new Vector2(125, 200);
        public Vector2 Position => renderData.Position;

        //
        public abstract string ModuleName { get; }

        protected ModuleNode(Vector2 position)
        {
            ModuleBase module = CreateModule();
            if(module != null)
                moduleData = new ModuleData(module, this);
            renderData = new NodeRenderData(this, position, NodeSize);
            IOData = new NodeIOData(this, Inputs, Outputs);
        }

        protected abstract ModuleBase CreateModule();

        public double GetValue(double x, double y, double z)
        {
            return moduleData.Module.GetValue(x, y, z);
        }

        public void Notify_DataChanged()
        {
            if(IOData.ConnectsTo(Window_ModuleVisualizer.Vis.FinalOutput))
                Window_ModuleVisualizer.Vis.Notify_DataChanged();
        }

        public void Notify_TryConnectAt(NodeAnchor fromAnchor, Vector2 lineEnd)
        {
            Window_ModuleVisualizer.Vis.TryConnectModuleToOther(this, fromAnchor, lineEnd);
        }

        public void Notify_MouseInputAt(Event mouseEvent)
        {
            renderData.DoMouseEvents(mouseEvent);
        }

        //
        public bool Contains(Vector2 pos)
        {
            return Renderer.Rect.Contains(pos);
        }

        public void Draw(float scale)
        {
            renderData.DrawNode(scale);
        }

        internal bool HasInputAt(Vector2 toPos, out NodeAnchor inputAnchor)
        {
            return IOData.HasInputAt(toPos, out inputAnchor);
        }

        public void Notify_NewConnection(NodeAnchor fromAnchor, NodeAnchor toAnchor)
        {
            fromAnchor.ConnectTo(toAnchor);
            toAnchor.ConnectTo(fromAnchor);
        }

        public void Notify_NewInput(int index, ModuleNode targetAnchorParentNode)
        {
            ModuleData?.Notify_SetNewInput(index, targetAnchorParentNode);
        }
    }
}
