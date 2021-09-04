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
    public class ModuleVisualizer : Window
    {
        private static int renderResultSize = 512;
        private float nodeGraphScale = 1f;
        private static List<Type> moduleTypesInt;


        public static float AnchorHeight = 20f;

        private List<ModuleNode> allNodes = new List<ModuleNode>();

        public static ModuleVisualizer Vis { get; private set; }
        public static List<Type> ModuleTypes => moduleTypesInt ??= typeof(ModuleNode).AllSubclassesNonAbstract();

        public bool MakingNewConnection { get; set; }

        public override Vector2 InitialSize => new Vector2(1600, 800);

        public OutputNode FinalOutput { get; private set; }


        public override void DoWindowContents(Rect inRect)
        {

            //
            DrawNodeGraph(inRect);

        }

        public override void PostOpen()
        {
            Vis = this;
            base.PostOpen();
        }

        public void SetNewConnection(int hash, Vector3 start, Vector3 end)
        {

        }

        public void RemoveConnection(int hash)
        {

        }

        private void DrawNodeGraph(Rect inRect)
        {
            //
            HandleNodeManipulation(inRect);

            //
            foreach (var node in allNodes){
                node.IOAnchors.DrawConnectionLine();
            }

            foreach (var node in allNodes)
            {
                node.Draw(nodeGraphScale);
            }

            if (FinalOutput is {FinalBase: { }})
            {
                Rect renderRect = new Rect(inRect.width - renderResultSize, 0, renderResultSize, renderResultSize);
                RenderModuleResult(renderRect, FinalOutput.FinalBase);
            }
        }

        private void HandleNodeManipulation(Rect inRect)
        {
            Event curEvent = Event.current;

            allNodes.ForEach(n => n.Notify_MouseInputAt(curEvent));

            if (curEvent.type == EventType.MouseDown && curEvent.button == 1)
            {
                var currentPos = curEvent.mousePosition;
                List<FloatMenuOption> list = new List<FloatMenuOption>();

                /*
                list.Add(new FloatMenuOption("Output", delegate
                {
                    allNodes.Add(new OutputNode(currentPos));
                }));
                list.Add(new FloatMenuOption("PERLIN", delegate
                { 
                    allNodes.Add(new PerlinNode(currentPos));
                }));
                list.Add(new FloatMenuOption("MULT", delegate
                {
                    allNodes.Add(new MultiplyNode(currentPos));
                }));
                */
                foreach (var type in ModuleTypes)
                {
                    list.Add( new FloatMenuOption(type.ToString().Split('.').Last(), delegate
                    {
                        allNodes.Add((ModuleNode)Activator.CreateInstance(type, currentPos));
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

        public void TryConnectModuleToOther(ModuleNode fromNode, NodeAnchor fromAnchor, Vector2 toPos)
        {
            NodeAnchor anchor = null;
            var nodeToConnect = allNodes.Find(n => n.HasInputAt(toPos, out anchor));
            if (nodeToConnect != null)
            {
                fromNode.Notify_NewConnection(fromAnchor, anchor);
                fromNode.Notify_DataChanged();
            }
        }

        public void RenderModuleResult(Rect inRect, ModuleBase result)
        {
            Widgets.DrawMenuSection(inRect);
            Widgets.DrawTextureFitted(inRect, GetTextureFrom(result), 1);
        }

        public bool NeedsReRender { get; private set; }

        private Texture2D CachedRenderTex { get; set; }

        private Texture2D GetTextureFrom(ModuleBase module)
        {
            if (!NeedsReRender)
            {
                return CachedRenderTex;
            }
            CachedRenderTex = new Texture2D(renderResultSize, renderResultSize, TextureFormat.RGBAFloat, false);
            for (int x = 0; x < renderResultSize; x++)
            {
                for (int y = 0; y < renderResultSize; y++)
                {
                    var val = (float)module.GetValue(x, 0, y);
                    CachedRenderTex.SetPixel(x, y, new Color(val, val, val));
                }
            }
            CachedRenderTex.wrapMode = TextureWrapMode.Clamp;
            CachedRenderTex.Apply();
            NeedsReRender = false;
            return CachedRenderTex;
        }

        public void Notify_NewOutput(OutputNode outputNode)
        {
            if (FinalOutput != null)
            {
                allNodes.Remove(FinalOutput);
            }
            FinalOutput = outputNode;
        }

        public void Notify_DataChanged()
        {
            NeedsReRender = true;
        }
    }
}
