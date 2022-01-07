using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class GraphicElement : UIElement
    {
        private Graphic graphic;
        private Rot4 curRot = Rot4.North;

        public Texture Texture => graphic.MatAt(curRot).mainTexture;

        public float rotation = 0;

        public override int Priority => 1;

        protected override Rect DragAreaRect => Rect;

        public KeyFrameData KeyFrameData { get; }
        public UIElement Owner => this;

        public GraphicElement(Graphic graphic) : base()
        {
            
            this.graphic = graphic;
            SetData(null, new Vector2(Texture.width, Texture.height));
            bgColor = Color.clear;
        }

        protected override void HandleEvent_Custom(Event ev, bool inContext)
        {

        }

        protected override void DrawContents(Rect inRect)
        {
            Widgets.DrawTextureFitted(inRect, Texture, 1f, new Vector2((float)Texture.width, (float)Texture.height), new Rect(0f, 0f, 1f, 1f), rotation);

            Rect rotateButt = new Rect(Position.x, Position.y, 15, 15);
            if (Widgets.ButtonText(rotateButt, "Rot"))
            {
                curRot = new Rot4(curRot.AsInt < 4 ? curRot.AsInt + 1 : 1);
            }
        }
    }
}
