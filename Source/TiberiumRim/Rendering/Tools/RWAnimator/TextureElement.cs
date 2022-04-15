using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
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
        protected TextureElement parentElement;
        protected List<TextureElement> subParts = new ();

        //Local texture manipulation
        private Vector2? oldPivot;
        private KeyFrameData? oldKF;
        private ManipulationMode? lockedInMode = null;

        //UIElement
        public UIElement Element => this;
        public UIElement Owner => this;
        public TextureCanvas ParentCanvas => (TextureCanvas)parent;
        public override bool CanBeFocused => base.CanBeFocused && IsSelected;
        protected override Rect DragAreaRect => Rect;
        public ManipulationMode ManiMode { get; private set; } = ManipulationMode.Move;
        public string[] ValueBuffer => RenderData.ValueBuffer;

        //Texture Data
        public KeyFrameData KeyFrameData => LocalData;
        private KeyFrameData RenderData => ParentCanvas.TimeLine.GetDataFor(this);
        private Vector2 RenderPivot => TruePos + (PivotPoint * ParentCanvas.CanvasZoomScale);

        public Rect TexCoords => texture.TexCoords;
        public List<TextureElement> SubParts => subParts;

        public ManipulationMode LockedInMode => lockedInMode ?? ManiMode;
        private bool ManiModeFlag => lockedInMode != null && lockedInMode != ManipulationMode.None;

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
                var oldPos = texture.TPosition;
                texture.TPosition = value;
                foreach (var t in SubParts)
                {
                    t.SetTRSP_Direct(t.TPosition + (value - oldPos));
                }
                ParentCanvas.TimeLine.UpdateKeyframeFor(this, LocalData);
            }
        }

        public float TRotation
        {
            get => RenderData.TRotation;
            set
            {
                var oldRot = TRotation;
                texture.TRotation = value;

                var rotDiff = value - oldRot;
                var pivot = texture.TPosition;
                foreach (var t in SubParts)
                {
                    var point = t.TPosition;
                    var result = ((Quaternion.Euler(0,0, rotDiff) * (point - pivot)) + (Vector3)pivot);
                    t.SetTRSP_Direct(result, rot: value);
                }
                ParentCanvas.TimeLine.UpdateKeyframeFor(this, LocalData);
            }
        }

        public Vector2 TSize
        {
            get => RenderData.TSize;
            set
            {
                var oldSize = TSize;
                texture.TSize = value;
                var sizeDiff = (value - oldSize);
                foreach (var t in SubParts)
                {
                    var vec = Vector2.one;
                    if (t.TPosition.x < TPosition.x)
                        vec.x = -1;
                    if (t.TPosition.y < TPosition.y)
                        vec.y = -1;

                    var offSet = new Vector2((t.TPosition.x / value.x), (t.TPosition.x / value.x));
                    if (sizeDiff.x < 0)
                    {
                        offSet.x = -offSet.x;
                    }
                    if (sizeDiff.y < 0)
                    {
                        offSet.y = -offSet.y;
                    }
                    offSet *= vec;

                    //TLog.Debug($"Settings TSize: {t.TPosition} + ({vec} * ({sizeDiff} * {(t.TPosition / 2f)})) => {t. + (vec * (sizeDiff * (t.TPosition)))}");
                    var newPos = t.TPosition + offSet;
                    t.SetTRSP_Direct(newPos, size:t.TSize + sizeDiff);
                }
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
            ParentCanvas.TimeLine.UpdateKeyframeFor(this, LocalData);
            UpdateBuffer();
        }

        public void SetTRSP_FromScreenSpace(Vector2? pos = null, float? rot = null, Vector2? size = null, Vector2? pivot = null)
        {
            TPosition = ParentCanvas.MousePos;
            TRotation = rot ?? RenderData.TRotation;
            TSize = size ?? RenderData.TSize;
            PivotPoint = pivot ?? PivotPoint;
            ParentCanvas.TimeLine.UpdateKeyframeFor(this, LocalData);
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

        public void LinkToParent(TextureElement newParent)
        {
            newParent.SubParts.Add(this);
            this.parentElement = newParent;
        }

        public void UnlinkFromParent()
        {
            if (parentElement != null)
            {
                parentElement.SubParts.Remove(this);
                parentElement = null;
            }
        }


        public override void Notify_RemovedFromContainer(UIContainer parent)
        {
            UnlinkFromParent();
            if (!subParts.NullOrEmpty())
            {
                subParts.Do(p => p.UnlinkFromParent());
            }
        }

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
                        TPosition = oldPos + dragDiff;
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
                        var newRot = Normalize(Mathf.FloorToInt(oldKF.Value.TRotation + Normalize(Mathf.FloorToInt(Vector2.SignedAngle(vec1, vec2)), 0, 360)), 0, 360);
                        TRotation = newRot;
                        break;
                }

                if (ManiModeFlag) //Update KeyFrame
                {
                    UpdateBuffer();
                }
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
            Widgets.DrawTextureFitted(RenderPivot.RectOnPos(new Vector2(24, 24)), TiberiumContent.PivotPoint, 1, Vector2.one, TexCoords, TRotation);
            GUI.color = Color.white;
        }

        protected override void DrawContents(Rect inRect)
        {
            TRWidgets.DrawTextureFromMat(TextureRect, RenderPivot, TRotation, Material, TexCoords);
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
            TRWidgets.DrawTextureWithMaterial(inRect, Texture, mat);
            Widgets.DrawTextureFitted(inRect, Texture, 1);
            TRWidgets.DoTinyLabel(inRect, $"{mat.mainTexture.name}");

            if (parentElement != null)
                GUI.color = Color.blue;

            if(!subParts.NullOrEmpty())
                GUI.color = Color.red;

            Rect rect = new Rect(inRect.xMax-32, inRect.y, 32, 32);
            if (Widgets.ButtonImage(rect, TiberiumContent.LinkIcon, GUI.color))
            {
                var floatOptions = new List<FloatMenuOption>();
                foreach (TextureElement tex in ParentCanvas.ElementList)
                {
                    if (tex != this)
                    {
                        floatOptions.Add(new FloatMenuOption($"{tex.Texture.name}", delegate
                        {
                            LinkToParent(tex);
                        }));
                    }
                }
                Find.WindowStack.Add(new FloatMenu(floatOptions));
            }

            GUI.color = Color.white;
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
