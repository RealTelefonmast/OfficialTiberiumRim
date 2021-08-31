using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace TiberiumRim
{
    public class NodeAnchor
    {
        private ModuleNode parentNode;
        private NodeAnchor targetAnchor;

        private bool isInput;

        //Render Data
        private Vector2 offset;
        private Rect anchorRect;

        public Vector2 ParentPos => parentNode.Position;
        public Vector2 Position => ParentPos + offset;

        private Vector2 Size => new Vector2(15, 15);

        public NodeAnchor(ModuleNode parent, Vector2 offSet, bool isInput)
        {
            parentNode = parent;
            this.offset = offSet;
            this.isInput = isInput;
        }

        public void ConnectTo(NodeAnchor other)
        {
            targetAnchor = other;
            if(isInput)
                other.ConnectTo(this);
        }

        public void DrawAnchor()
        {
            //
            DoAnchorControl();

            //
            var offset = Size.x / 2;
            anchorRect = new Rect(Position.x - offset, Position.y - offset, Size.x, Size.y);
            Widgets.DrawMenuSection(anchorRect);

            if (isInput)
            {
                if (Mouse.IsOver(anchorRect))
                {
                    GUI.color = Color.red;
                    Widgets.DrawHighlight(anchorRect);
                    GUI.color = Color.white;
                }
                return;
            }

            if(Mouse.IsOver(anchorRect))
                Widgets.DrawHighlight(anchorRect);

            if (targetAnchor != null)
            {
                Widgets.DrawLine(Position, targetAnchor.Position, Color.red, 4);
            }

            if (currentLineEnd != null)
            {
                Widgets.DrawLine(Position, currentLineEnd.Value, Color.cyan, 4); 
            }
        }

        private Vector2? currentLineEnd;
        private bool makingLine = false;

        private void DoAnchorControl()
        {
            var curEv = Event.current;
            var evType = curEv.type;
            if (Mouse.IsOver(anchorRect))
            {
                if (evType == EventType.MouseDown)
                {
                    //Start connecting
                    makingLine = true;
                }
            }

            if (evType == EventType.MouseDrag && makingLine)
            {
                //Update line
                currentLineEnd = Event.current.mousePosition;
            }

            if (evType == EventType.MouseUp && curEv.button == 0)
            {
                //Stop connecting
                if (currentLineEnd == null) return;
                parentNode.Notify_TryConnectAt(currentLineEnd.GetValueOrDefault());
                currentLineEnd = null;
                makingLine = false;
            }
        }

        internal bool RectContains(Vector2 toPos)
        {
            return anchorRect.Contains(toPos);
        }
    }

    public class ModuleNode
    {
        private ModuleBase internalModule;
        private ModuleData internalData;

        private ModuleNode[] inputNodes;
        private Traverse modules;

        //Render Data
        private Vector2 position;
        private Rect curRect;

        public NodeAnchor[] InputAnchors { get; private set; }
        public NodeAnchor OutputAnchor { get; private set; }

        //Graphical
        public Vector2 Size => new Vector2(100, 150);
        public Vector2 Position => position;
        public Rect NodeRect => curRect;

        //Node Data
        public ModuleBase Output => internalModule;
        public ModuleBase[] Inputs
        {
            get => modules.GetValue<ModuleBase[]>();
            private set => modules.SetValue(value);
        }

        public ModuleNode(ModuleBase module, Vector2 position)
        {
            this.position = position;
            internalModule = module;
            internalData = new ModuleData(internalModule);
            modules = Traverse.Create(internalModule).Field("modules");
            inputNodes = new ModuleNode[internalModule.SourceModuleCount];
            SetAnchors();
        }

        public double GetValue(double x, double y, double z)
        {
            return Output.GetValue(x, y, z);
        }

        //Manipulation
        private Vector2? startDraggingPos;
        private Vector2? oldPos;

        private void SetAnchors()
        {
            var val = Size.y / 3f;
            InputAnchors = new NodeAnchor[] {new NodeAnchor(this, new Vector2(0, val), true), new NodeAnchor(this, new Vector2(0, val * 2), true)};
            OutputAnchor = new NodeAnchor(this, new Vector2(Size.x, Size.y / 2f),false);
            //InputAnchors = new Vector2[] { position + new Vector2(0, val), position + new Vector2(0, val * 3) };
            //OutputAnchor = position + new Vector2(Size.x, Size.y / 2f);
        }

        public void TryHandleMouseInput(Event mouseEvent)
        {
            Vector2 mousePos = mouseEvent.mousePosition;
            if (!NodeRect.Contains(mousePos)) return;
            if (mouseEvent.type == EventType.MouseDrag)
            {
                startDraggingPos ??= mousePos;
                oldPos ??= position;
                Vector2 diff = mousePos - startDraggingPos.Value;
                position = new Vector2(oldPos.Value.x + diff.x, oldPos.Value.y + diff.y);
            }

            if (mouseEvent.type == EventType.MouseUp)
            {
                ResetInput();
            }
        }

        //
        public bool Contains(Vector2 pos)
        {
            return curRect.Contains(pos);
        }

        public void Draw(float scale)
        {
            curRect = new Rect(position.x, position.y, Size.x, Size.y);
            Widgets.DrawMenuSection(curRect);
            internalData.DrawData(curRect);

            foreach (var inputAnchor in InputAnchors)
            {
                inputAnchor.DrawAnchor();
            }
            OutputAnchor.DrawAnchor();
        }

        public void ResetInput()
        {
            startDraggingPos = null;
            oldPos = null;
        }

        public void Notify_TryConnectAt(Vector2 lineEnd)
        {
            ModuleVisualizer.Vis.TryConnectModuleToOther(this, lineEnd);
        }

        internal bool HasInputAt(Vector2 toPos, out NodeAnchor inputAnchor)
        {
            inputAnchor = InputAnchors.FirstOrFallback(n => n.RectContains(toPos));
            return inputAnchor != null;
        }
    }

    public class ModuleVisualizer : Window
    {
        private static int renderResultSize = 1024;
        private static List<Type> moduleTypesInt;

        private List<ModuleNode> allNodes = new List<ModuleNode>();

        public static ModuleVisualizer Vis { get; private set; }

        public override Vector2 InitialSize => new Vector2(1600, 800);

        public static List<Type> ModuleTypes => moduleTypesInt ??= typeof(ModuleBase).AllSubclassesNonAbstract(); 

        public override void DoWindowContents(Rect inRect)
        {

            //
            DrawNodeGraph(inRect);

        }

        private Vector2 originVec = new Vector2(0,0);
        private float nodeGraphScale = 1f;

        public override void PostOpen()
        {
            Vis = this;
            base.PostOpen();
        }

        private void DrawNodeGraph(Rect inRect)
        {
            //
            HandleNodeManipulation(inRect);

            //
            foreach (var node in allNodes)
            {
                node.Draw(nodeGraphScale);
            }

        }

        private void HandleNodeManipulation(Rect inRect)
        {
            Event curEvent = Event.current;

            allNodes.ForEach(n => n.TryHandleMouseInput(curEvent));

            if (curEvent.type == EventType.MouseDown && curEvent.button == 1)
            {
                var currentPos = curEvent.mousePosition;
                List<FloatMenuOption> list = new List<FloatMenuOption>();
                
                list.Add(new FloatMenuOption("PERLIN", delegate
                { 
                    allNodes.Add(new ModuleNode(new Perlin(0.03, 3, 0.4, 6, Rand.Range(0, int.MaxValue), QualityMode.High), currentPos));
                }));
                list.Add(new FloatMenuOption("MULT", delegate
                {
                    allNodes.Add(new ModuleNode(new Multiply(null, null), currentPos));
                }));
                
                foreach (var type in ModuleTypes)
                {
                    list.Add( new FloatMenuOption(type.ToString().Split('.').Last(), delegate
                    {
                        allNodes.Add(new ModuleNode((ModuleBase)Activator.CreateInstance(type), currentPos));
                    }));
                }
                Find.WindowStack.Add(new FloatMenu(list));
            }

            /*
            if (eventType == EventType.MouseUp)
            {
                allNodes.ForEach(n => n.ResetInput());
                return;
            }
            var node = allNodes.First(n => n.NodeRect.Contains(curEvent.mousePosition)); 
            node.HandleMouseInput(curEvent);
            */
        }

        public void TryConnectModuleToOther(ModuleNode fromNode, Vector2 toPos)
        {
            NodeAnchor anchor = null;
            var nodeToConnect = allNodes.Find(n => n.HasInputAt(toPos, out anchor));
            if (nodeToConnect != null)
            {
                anchor.ConnectTo(fromNode.OutputAnchor);
            }
        }

        public void TryCreateNode()
        {

        }

        public void RenderModuleResult(Rect inRect, ModuleBase result)
        {

        }

        private void GetTextureFrom(ModuleBase module)
        {

        }
    }
}
