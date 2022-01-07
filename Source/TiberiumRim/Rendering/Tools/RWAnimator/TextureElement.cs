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
        //
        private Material materialInt;

        //
        private KeyFrameData localData;
        public readonly Rect? texCoords;
        public Vector2 pivotPoint = Vector2.zero;

        //Local texture manipulation
        protected Vector2? oldPivot;
        protected KeyFrameData? oldKF;

        //UIElement
        public UIElement Element => this;
        public UIElement Owner => this;
        public TextureCanvas ParentCanvas => (TextureCanvas)parent;
        public override UIElementMode UIMode => UIElementMode.Static;
        protected override Rect DragAreaRect => Rect;
        public ManipulationMode ManiMode { get; private set; } = ManipulationMode.Move;

        //Texture Data
        public KeyFrameData KeyFrameData => localData;
        private KeyFrameData RenderData => ParentCanvas.TimeLine.GetDataFor(this) ?? localData;

        private Vector2 RenderPivot => TruePos + pivotPoint;

        public Vector2 PivotPoint
        {
            get => pivotPoint;
            set => pivotPoint = value;
        }

        public Vector2 TPosition
        {
            get => RenderData.position;
            set => localData.position = value;
        }

        public float TRotation
        {
            get => RenderData.rotation;
            set => localData.rotation = value;
        }

        public Vector2 TSize
        {
            get => RenderData.size;
            set => localData.size = value;
        }

        private bool IsSelected => ParentCanvas.ActiveTexture == this;
        private float RotateDistance => 1.15f * TSize.x;

        //
        private Vector2 ZoomedSize => TSize * ParentCanvas.CanvasZoomScale;
        private Vector2 ZoomedPos => TPosition * ParentCanvas.CanvasZoomScale;

        private Vector2 TruePos => ParentCanvas.Origin + (ZoomedPos);
        public Vector2 RectPosition => TruePos - ZoomedSize / 2f;

        public Rect TextureRect => new Rect(RectPosition, ZoomedSize);
        public override Rect FocusRect => TextureRect.ExpandedBy(15);

        public Material Material => materialInt;
        public Texture Texture => Material.mainTexture;
        public Vector2 TextureSize => new Vector2(materialInt.mainTexture.width, materialInt.mainTexture.height);

        public TextureElement(Rect rect, Texture texture) : base(rect)
        {
            bgColor = Color.clear;
            materialInt = new Material(ShaderDatabase.CutoutComplex);
            materialInt.mainTexture = texture;
            materialInt.color = Color.white;

            TPosition = Vector3.zero;
            TRotation = 0;
            TSize = TextureSize / 2f;

            hasTopBar = false;
        }

        public TextureElement(Rect rect, Material mat, Rect texCoords) : base(rect)
        {
            bgColor = Color.clear;
            materialInt = mat;
            this.texCoords ??= texCoords;

            TPosition = Vector3.zero;
            TRotation = 0;
            TSize = TextureSize / 2f;

            hasTopBar = false;
        }

        public void SetTRS(KeyFrameData? data)
        {
            if (data.HasValue)
            {
                var frame = data.Value;
                SetTRSP_Direct(frame.position, frame.rotation, frame.size);
            }
        }

        public void SetTRSP_Direct(Vector2? pos = null, float? rot = null, Vector2? size = null, Vector2? pivot = null)
        {
            TPosition = pos ?? RenderData.position;
            TRotation = rot ?? RenderData.rotation;
            TSize = size ?? RenderData.size;
            PivotPoint = pivot ?? PivotPoint;
        }

        public void SetTRSP_FromScreenSpace(Vector2? pos = null, float? rot = null, Vector2? size = null, Vector2? pivot = null)
        {
            TPosition = ParentCanvas.MousePos;
            TRotation = rot ?? RenderData.rotation;
            TSize = size ?? RenderData.size;
            PivotPoint = pivot ?? PivotPoint;
        }

        public void Reset()
        {
            TSize = TextureSize / 2f;
            TRotation = 0;
            PivotPoint = Vector2.zero;
            ParentCanvas.TimeLine.UpdateKeyframeFor(this, localData);
        }

        private Vector2? CanvasPosToOffset(Vector2? canvasPos)
        {
            return ((canvasPos - ParentCanvas.InRect.position) - ParentCanvas.TrueOrigin) / ParentCanvas.CanvasZoomScale;
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
                    default: return IsSelected ? Color.cyan : TRMats.White005;
                }
            }
        }

        private ManipulationMode? lockedInMode = null;

        private bool ManiModeFlag => lockedInMode != null && lockedInMode != ManipulationMode.None;

        protected override void HandleEvent_Custom(Event ev, bool inContext)
        {
            if (!IsSelected) return;

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
                    oldPivot ??= pivotPoint;

                    if (ManiModeFlag) //Update Local
                    {
                        localData = oldKF.Value;
                        UIEventHandler.StartFocus(this);
                    }
                }
            }

            if (ev.type == EventType.MouseDrag)
            {
                var dragDiff = CurrentDragDiff;
                switch (lockedInMode)
                {
                    case ManipulationMode.PivotDrag:
                        var oldPivPos = oldPivot.Value;
                        PivotPoint = new Vector2(oldPivPos.x + dragDiff.x, oldPivPos.y + dragDiff.y);
                        break;
                    case ManipulationMode.Move:
                        //if (!TextureRect.Contains(mv) || !IsFocused) return;
                        dragDiff /= ParentCanvas.CanvasZoomScale;
                        var oldPos = oldKF.Value.position;
                        TPosition = new Vector2(oldPos.x + dragDiff.x, oldPos.y + dragDiff.y);
                        break;
                    case ManipulationMode.Resize:
                        TSize = oldKF.Value.size + dragDiff;
                        break;
                    case ManipulationMode.Rotate:
                        var vec1 = StartDragPos - RenderPivot;
                        var vec2 = ev.mousePosition - RenderPivot;
                        TRotation = Normalize(Mathf.FloorToInt(oldKF.Value.rotation + Normalize(Mathf.FloorToInt(Vector2.SignedAngle(vec1, vec2)),0, 360)), 0, 360);
                        break;
                }
                if (ManiModeFlag) //Update KeyFrame
                    ParentCanvas.TimeLine.UpdateKeyframeFor(this, localData);
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
            yield return new FloatMenuOption("Center", delegate
            {
                TPosition = Vector2.zero;
                ParentCanvas.TimeLine.UpdateKeyframeFor(this, localData);
            });
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
            TRWidgets.DrawTextureFromMat(TextureRect, RenderPivot, TRotation, materialInt, texCoords ?? default);
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
    }
}
