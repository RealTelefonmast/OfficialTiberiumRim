using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace TiberiumRim
{
    public class NodeRenderData
    {
        private readonly ModuleNode parent;
        private readonly Vector2 size;

        private Vector2 position;
        //private Rect rectInt;

        //Manipulation
        private Vector2? startDraggingPos;
        private Vector2? oldPos;

        public Vector2 Position => Position;
        public Vector2 Size
        {
            get
            {
                var val = 25 + parent.IOAnchors.Height;
                if(parent.ModuleData != null)
                    val += (size.y - val);

                return new Vector2(size.x, val);
            }
        }

        public Rect Rect => new Rect(position.x, position.y, Size.x, Size.y);

        public Vector2 LeftAnchor => new Vector2(position.x, position.y + size.y / 2);
        public Vector2 RightAnchor => new Vector2(position.x + size.x, position.y + size.y / 2);

        public NodeRenderData(ModuleNode parent, Vector2 originPos, Vector2 size)
        {
            this.parent = parent;
            this.position = originPos;
            this.size = size;
        }

        public void DrawNode(float scale)
        {
            Widgets.DrawMenuSection(Rect);
            //Rect TopHalf = Rect.TopPartPixels();


            Rect TopBarRect = new Rect(Rect.x, Rect.y, Rect.width, 25);
            Rect IORect = new Rect(Rect.x, TopBarRect.yMax, Rect.width, parent.IOAnchors.Height);
            Rect DataRect = new Rect(Rect.x, IORect.yMax, Rect.width, Rect.height - (TopBarRect.height + IORect.height));

            //Top Bar (Title/Drag)
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(TopBarRect, parent.ModuleName);
            Text.Anchor = default;
            Text.Font = GameFont.Small;

            //I/O
            parent.IOAnchors.DrawIO(IORect);

            //Data
            parent.ModuleData?.DrawData(DataRect);
        }

        public void DoMouseEvents(Event mouseEvent)
        {
            Vector2 mousePos = mouseEvent.mousePosition;
            if (!Rect.Contains(mousePos) && startDraggingPos == null || ModuleVisualizer.Vis.MakingNewConnection) return;
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

        private void ResetInput()
        {
            startDraggingPos = null;
            oldPos = null;
        }
    }
}
