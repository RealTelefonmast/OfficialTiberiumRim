using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public enum ManipulationMode
    {
        None,
        Move,
        Resize,
        Rotate,
        PivotDrag
    }

    public class TextureElement : UIElement, IKeyFramedElement, IReorderableElement
    {
        protected TextureData texture;

        //Local texture manipulation
        private Vector2? oldPivot;
        private KeyFrameData? oldKF;

        //UIElement
        public UIElement Element => this;
        public UIElement Owner => this;
        public TextureCanvas ParentCanvas => (TextureCanvas)parent;
        public override bool CanBeFocused => base.CanBeFocused && IsSelected;
        protected override Rect DragAreaRect => Rect;
        public ManipulationMode ManiMode { get; private set; } = ManipulationMode.Move;

        //Texture Data
        public KeyFrameData KeyFrameData => LocalData;
        private KeyFrameData RenderData => ParentCanvas.TimeLine.GetDataFor(this);
        private Vector2 RenderPivot => TruePos + (PivotPoint * ParentCanvas.CanvasZoomScale);
        private Rect? TexCoords => texture.TexCoords;

        public string[] ValueBuffer => RenderData.ValueBuffer;

        public KeyFrameData LocalData
        {
            get => texture.LocalData;
            set
            {
                texture.LocalData = value;
                ParentCanvas.TimeLine.UpdateKeyframeFor(this, value);
            }
        }

        public Vector2 PivotPoint
        {
            get => texture.PivotPoint;
            set
            {
                texture.PivotPoint = value;
                ParentCanvas.TimeLine.UpdateKeyframeFor(this, LocalData);
            }
        }

        public Vector2 TPosition
        {
            get => RenderData.TPosition;
            set
            {
                texture.TPosition = value;
                ParentCanvas.TimeLine.UpdateKeyframeFor(this, LocalData);
            }
        }

        public float TRotation
        {
            get => RenderData.TRotation;
            set
            {
                texture.TRotation = value;
                ParentCanvas.TimeLine.UpdateKeyframeFor(this, LocalData);
            }
        }

        public Vector2 TSize
        {
            get => RenderData.TSize;
            set
            {
                texture.TSize = value;
                ParentCanvas.TimeLine.UpdateKeyframeFor(this, LocalData);
            }
        }

        public void UpdateBuffer() => LocalData.UpdateBuffer();

        public Vector2 DrawSize => RenderData.TSize * texture.TSizeFactor;

        private bool IsSelected => ParentCanvas.ActiveTexture == this;
        private float RotateDistance => 1.15f * TSize.x;

        //
        private Vector2 ZoomedSize => DrawSize * ParentCanvas.CanvasZoomScale;
        private Vector2 ZoomedPos => TPosition * ParentCanvas.CanvasZoomScale;

        private Vector2 TruePos => ParentCanvas.Origin + (ZoomedPos);
        public Vector2 RectPosition => TruePos - ZoomedSize / 2f;

        public Rect TextureRect => new Rect(RectPosition, ZoomedSize);
        public override Rect FocusRect => TextureRect.ExpandedBy(15);

        public Material Material => texture.Material;
        public Texture Texture => Material.mainTexture;

        public TextureElement(Rect rect, WrappedTexture texture) : base(rect, UIElementMode.Dynamic)
        {
            bgColor = Color.clear;
            this.texture = new TextureData(texture);
            this.texture.SetTRS(Vector2.zero, 0, Vector2.one);

            hasTopBar = false;
        }

        public TextureElement(Rect rect, Material mat, Rect texCoords) : base(rect, UIElementMode.Dynamic)
        {
            bgColor = Color.clear;
            texture = new TextureData(mat);
            texture.SetTRS(Vector2.zero, 0, Vector2.one);
            texture.SetTexCoords(texCoords);

            hasTopBar = false;
        }

        public void SetTRS(KeyFrameData? data)
        {
            if (data.HasValue)
            {
                var frame = data.Value;
                SetTRSP_Direct(frame.TPosition, frame.TRotation, frame.TSize);
            }
        }

        public void SetTRSP_Direct(Vector2? pos = null, float? rot = null, Vector2? size = null, Vector2? pivot = null)
        {
            TPosition = pos ?? RenderData.TPosition;
            TRotation = rot ?? RenderData.TRotation;
            TSize = size ?? RenderData.TSize;
            PivotPoint = pivot ?? PivotPoint;
            UpdateBuffer();
        }

        public void SetTRSP_FromScreenSpace(Vector2? pos = null, float? rot = null, Vector2? size = null, Vector2? pivot = null)
        {
            TPosition = ParentCanvas.MousePos;
            TRotation = rot ?? RenderData.TRotation;
            TSize = size ?? RenderData.TSize;
            PivotPoint = pivot ?? PivotPoint;
            UpdateBuffer();
        }

        public void Reset()
        {
            TSize = Vector2.one;
            TRotation = 0;
            PivotPoint = Vector2.zero;
            ParentCanvas.TimeLine.UpdateKeyframeFor(this, LocalData);
            UpdateBuffer();
        }

        public void Recenter()
        {
            TPosition = Vector2.zero;
            ParentCanvas.TimeLine.UpdateKeyframeFor(this, LocalData);
            UpdateBuffer();
        }

        public ManipulationMode LockedInMode => lockedInMode ?? ManiMode;

        public Color BorderColor
        {
            get
            {
                switch (lockedInMode ?? ManiMode)
                {
                    case ManipulationMode.Move:
                        return Color.blue;
                    case ManipulationMode.Resize:
                        return Color.green;
                    case ManipulationMode.Rotate:
                        return Color.magenta;
                    default: return IsSelected ? Color.cyan : TRColor.White005;
                }
            }
        }

        private ManipulationMode? lockedInMode = null;

        private bool ManiModeFlag => lockedInMode != null && lockedInMode != ManipulationMode.None;

        protected override void HandleEvent_Custom(Event ev, bool inContext)
        {
            if (!(IsSelected)) return;

            var mv = ev.mousePosition;
            var dist = mv.DistanceToRect(TextureRect);
            var pivotDist = Vector2.Distance(mv, RenderPivot);

            ManiMode = ManipulationMode.None;
            if (pivotDist <= 8)
            {
                ManiMode = ManipulationMode.PivotDrag;
            }
            else if (TextureRect.Contains(mv))
            {
                ManiMode = ManipulationMode.Move;
            }

            if (dist is > 0 and < 5)
            {
                ManiMode = ManipulationMode.Resize;
            }

            if (dist is > 5 and < 15)
            {
                ManiMode = ManipulationMode.Rotate;
            }

            if (ev.type == EventType.MouseDown)
            {
                if (ev.button == 0)
                {
                    lockedInMode ??= ManiMode;
                    oldKF ??= RenderData;
                    oldPivot ??= PivotPoint;

                    if (ManiModeFlag) //Update Local
                    {
                        LocalData = oldKF.Value;
                        UIEventHandler.StartFocusForced(this);
                    }
                }
            }

            if (IsFocused && ev.type == EventType.MouseDrag)
            {
                var dragDiff = CurrentDragDiff;
                switch (lockedInMode)
                {
                    case ManipulationMode.PivotDrag:
                        var oldPivPos = oldPivot.Value;
                        dragDiff /= ParentCanvas.CanvasZoomScale;
                        PivotPoint = new Vector2(oldPivPos.x + dragDiff.x, oldPivPos.y + dragDiff.y);
                        break;
                    case ManipulationMode.Move:
                        //if (!TextureRect.Contains(mv) || !IsFocused) return;
                        var oldPos = oldKF.Value.TPosition;
                        dragDiff /= ParentCanvas.CanvasZoomScale;
                        TPosition = new Vector2(oldPos.x + dragDiff.x, oldPos.y + dragDiff.y);
                        break;
                    case ManipulationMode.Resize:
                        dragDiff *= 2;
                        dragDiff /= texture.TSizeFactor;
                        dragDiff /= ParentCanvas.CanvasZoomScale;

                        var norm = (StartDragPos - TextureRect.center).normalized;

                        if (norm.x < 0)
                            dragDiff = new Vector2(-dragDiff.x, dragDiff.y);
                        if (norm.y < 0)
                            dragDiff = new Vector2(dragDiff.x, -dragDiff.y);

                        TSize = oldKF.Value.TSize + dragDiff;
                        break;
                    case ManipulationMode.Rotate:
                        var vec1 = StartDragPos - RenderPivot;
                        var vec2 = ev.mousePosition - RenderPivot;
                        TRotation = Normalize(Mathf.FloorToInt(oldKF.Value.TRotation + Normalize(Mathf.FloorToInt(Vector2.SignedAngle(vec1, vec2)),0, 360)), 0, 360);
                        break;
                }

                if (ManiModeFlag) //Update KeyFrame
                    UpdateBuffer();
                //ParentCanvas.TimeLine.UpdateKeyframeFor(this, LocalData);
            }

            if (ev.type == EventType.MouseUp)
            {
                oldKF = null;
                oldPivot = null;
                lockedInMode = null;
            }
        }

        protected override IEnumerable<FloatMenuOption> RightClickOptions()
        {
            yield return new FloatMenuOption("Reset", this.Reset);
            yield return new FloatMenuOption("Delete", delegate { ParentCanvas.Discard(this); });
            yield return new FloatMenuOption("Center", Recenter);
        }

        private int Normalize(int value, int start, int end)
        {
            int width = end - start;
            int offsetValue = value - start;
            return (offsetValue - ((offsetValue / width) * width)) + start;
        }

        public void DrawSelOverlay()
        {
            TRWidgets.DrawBox(TextureRect, BorderColor, 1);

            //Draw Pivot
            GUI.color = ManiMode == ManipulationMode.PivotDrag ? Color.red : Color.white;
            Widgets.DrawTextureFitted(RenderPivot.RectOnPos(new Vector2(24, 24)), TiberiumContent.PivotPoint, 1, Vector2.one, new(0, 0, 1, 1), TRotation);
            GUI.color = Color.white;
        }

        protected override void DrawContents(Rect inRect)
        {
            TRWidgets.DrawTextureFromMat(TextureRect, RenderPivot, TRotation, Material, TexCoords ?? default);
            //GUI.DrawTextureWithTexCoords(TextureRect, texture, texCoords);

            if (TRotation != 0 && IsSelected)
            {
                var matrix = GUI.matrix;
                UI.RotateAroundPivot(TRotation, TextureRect.center);
                TRWidgets.DrawBoxHighlight(TextureRect);
                GUI.matrix = matrix;
            }

            TRWidgets.DrawBox(TextureRect, BorderColor, 1);

            //var mv = Event.current.mousePosition;
            //TRWidgets.DoTinyLabel(TextureRect, $"Local\n{KeyFrameData.position}\n{KeyFrameData.rotation}\n{KeyFrameData.size}\n{Vector2.Distance(mv, PivotPoint)}\n{mv.DistanceToRect(TextureRect)}");
            //TRWidgets.DoTinyLabel(TextureRect.RightPartPixels(80), $"KeyLerp\n{RenderData.position}\n{RenderData.rotation}\n{RenderData.size}");
        }

        public void DrawElementInScroller(Rect inRect)
        {
            var mat = Material;
            Widgets.DrawTextureFitted(inRect, Texture, 1);
            TRWidgets.DoTinyLabel(inRect, $"{mat.mainTexture.name}");
            //TRWidgets.DoTinyLabel(inRect, $"{mat.mainTexture.name}\n{mat.shader.name}\n{RectSimple(texCoords ?? default)}\n{pivotPoint}\n{mat.mainTextureOffset}\n{mat.mainTextureScale}");
        }

        private string RectSimple(Rect rect)
        {
            return $"({rect.x},{rect.y});({rect.width},{rect.height})";
        }

        public TextureData GetData()
        {
            return texture;
        }
    }
}
