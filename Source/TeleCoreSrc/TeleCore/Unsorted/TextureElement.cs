using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace TeleCore.Unsorted;

public enum ManipulationMode
{
    None,
    Move,
    Resize,
    Rotate,
    PivotDrag
}

public enum TexStretchMode
{
    Normal,
    StretchToFit
}

public enum TexCoordAnchor
{
    Center,
    Top,
    Bottom,
    Left,
    Right,
    TopLeft,
    TopRight,
    BottomLeft,
    BottomRight
}

internal class TextureElement : UIElement, IKeyFramedElement, IReorderableElement
{
    private ManipulationMode? lockedInMode;
    private KeyFrameData? oldKF;

    //Local texture manipulation
    private Vector2? oldPivot;
    protected TextureElement parentElement;

    protected List<TextureElement> subParts = new();

    //
    private readonly string[] tempBuffer = new string[AnimationMetaData.BufferSize];

    //
    protected TextureData texture;

    public TextureElement(Rect rect, TextureData textureData) : base(rect, UIElementMode.Dynamic)
    {
        bgColor = Color.clear;
        texture = textureData;
        texture.LayerTag = Material.mainTexture.name;

        hasTopBar = false;
    }

    public TextureElement(Rect rect, WrappedTexture texture) : base(rect, UIElementMode.Dynamic)
    {
        bgColor = Color.clear;
        this.texture = new TextureData(texture);
        this.texture.LayerTag = Material.mainTexture.name;

        hasTopBar = false;
    }

    public TextureElement(Rect rect, Material mat, Rect texCoords) : base(rect, UIElementMode.Dynamic)
    {
        bgColor = Color.clear;
        texture = new TextureData(mat);
        texture.SetTexCoordReference(texCoords);
        texture.LayerTag = Material.mainTexture.name;
        hasTopBar = false;
    }

    public TextureCanvas ParentCanvas => (TextureCanvas) _parent;
    public override bool CanBeFocused => base.CanBeFocused && IsSelected;
    protected override Rect DragAreaRect => Rect;
    public ManipulationMode ManiMode { get; private set; } = ManipulationMode.None;
    public KeyFrame CurrentFrame => ParentCanvas.TimeLine.GetKeyFrame(this);
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
                default: return IsSelected ? Color.cyan : TColor.White005;
            }
        }
    }

    // IMPORTANT: Add UpdateKeyframeFor to KeyFrameData changes!
    public Vector2 PivotPoint
    {
        get => CurrentData.PivotPoint;
        set
        {
            var tempFrame = CurrentData;
            var oldPivot = tempFrame.PivotPoint;
            tempFrame.PivotPoint = value;
            var pivotDiff = value - oldPivot;

            //
            ParentCanvas.TimeLine.UpdateKeyframeFor(this, tempFrame);
            if (Input.GetKey(KeyCode.LeftShift))
                ParentCanvas.TimeLine.UpdateAllFramesFor(this, ManipulationMode.PivotDrag, pivotDiff);
        }
    }

    public Rect TexCoords
    {
        get => CurrentData.TexCoords;
        set
        {
            var tempFrame = CurrentData;
            tempFrame.TexCoords = value;

            //
            ParentCanvas.TimeLine.UpdateKeyframeFor(this, tempFrame);
        }
    }

    public TexCoordAnchor TexCoordAnchor
    {
        get => texture.TexCoordAnchor;
        set => texture.TexCoordAnchor = value;
    }

    public TexStretchMode StretchMode
    {
        get => texture.StretchMode;
        set => texture.StretchMode = value;
    }

    public bool AttachScript
    {
        get => texture.AttachScript;
        set => texture.AttachScript = value;
    }

    public string LayerTag
    {
        get => texture.LayerTag;
        set => texture.LayerTag = value;
    }

    public string StringBuffer
    {
        get => texture.StringBuffer;
        set => texture.StringBuffer = value;
    }

    public int LayerIndex
    {
        get => texture.LayerIndex;
        set => texture.LayerIndex = value;
    }

    public Vector2 RelatedPosition => TPosition + (parentElement?.TPosition ?? Vector2.zero);

    public float RelatedRotation => parentElement?.TRotation ?? 0;

    public Vector2 TPosition
    {
        get => CurrentData.TPosition;
        set
        {
            var tempFrame = CurrentData;
            var oldPos = tempFrame.TPosition;
            tempFrame.TPosition = value;
            var posDiff = value - oldPos;

            //
            foreach (var t in SubParts) t.SetTRSP_Direct(t.TPosition + posDiff);

            //
            ParentCanvas.TimeLine.UpdateKeyframeFor(this, tempFrame);
            if (Input.GetKey(KeyCode.LeftShift))
                ParentCanvas.TimeLine.UpdateAllFramesFor(this, ManipulationMode.Move, posDiff);
        }
    }

    public float TRotation
    {
        get => CurrentData.TRotation;
        set
        {
            var tempFrame = CurrentData;
            var oldRot = tempFrame.TRotation;
            tempFrame.TRotation = value;

            var rotDiff = value - oldRot;
            var pivot = tempFrame.TPosition + tempFrame.PivotPoint;
            foreach (var t in SubParts)
            {
                var point = t.TPosition + t.PivotPoint;
                var result = Quaternion.Euler(0, 0, rotDiff) * (point - pivot) + (Vector3) pivot -
                             (Vector3) t.PivotPoint;
                t.SetTRSP_Direct(result, t.TRotation + rotDiff);
            }

            //
            ParentCanvas.TimeLine.UpdateKeyframeFor(this, tempFrame);
            if (Input.GetKey(KeyCode.LeftShift))
                ParentCanvas.TimeLine.UpdateAllFramesFor(this, ManipulationMode.Rotate, rotDiff);
        }
    }

    public Vector2 TSize
    {
        get => CurrentData.TSize;
        set
        {
            var tempFrame = CurrentData;
            var oldSize = tempFrame.TSize;
            var newVal = value.Clamp(new Vector2(BaseCanvas.SizeRange.min, BaseCanvas.SizeRange.min),
                new Vector2(BaseCanvas.SizeRange.max, BaseCanvas.SizeRange.max));
            tempFrame.TSize = newVal;
            var sizeDiff = newVal - oldSize;
            foreach (var t in SubParts)
            {
                var tPosOff = t.TPosition - TPosition;
                var offSet = new Vector2(tPosOff.x / oldSize.x, tPosOff.y / oldSize.y) * sizeDiff;
                var newPos = t.TPosition + offSet;
                t.SetTRSP_Direct(newPos, size: t.TSize + sizeDiff);
            }

            //
            ParentCanvas.TimeLine.UpdateKeyframeFor(this, tempFrame);
            if (Input.GetKey(KeyCode.LeftShift))
                ParentCanvas.TimeLine.UpdateAllFramesFor(this, ManipulationMode.Resize, sizeDiff);
        }
    }

    public string[] ValueBuffer
    {
        get
        {
            if (IsAtKeyFrame) return ParentCanvas.AnimationData.BufferFor(CurrentFrame);
            CurrentData.UpdateBuffer(tempBuffer);
            return tempBuffer;
        }
    }

    //
    public Vector2 TSizeFactor => BaseCanvas.TileVector * texture.TexCoordReference.size;
    public Vector2 DrawSize => CurrentData.TSize * TSizeFactor;

    private bool IsSelected => ParentCanvas.ActiveTexture == this;
    private bool IsAtKeyFrame => ParentCanvas.TimeLine.IsAtKeyFrame(this, out _);

    //
    public bool ShowTexCoordGhost { get; set; }
    public bool Visibility { get; set; } = true;

    //
    private Vector2 ZoomedSize => DrawSize * ParentCanvas.CanvasZoomScale;
    private Vector2 ZoomedPos => TPosition * ParentCanvas.CanvasZoomScale;

    private Vector2 TruePos => ParentCanvas.Origin + ZoomedPos;
    private Vector2 RenderPivot => TruePos + PivotPoint * ParentCanvas.CanvasZoomScale;

    public Vector2 RectPosition => TruePos - ZoomedSize / 2f;
    public Vector2 RectPivotPosition => RenderPivot - ZoomedSize / 2f;

    public Rect TextureRect => new(RectPosition, ZoomedSize);
    public Rect TextureOverlayRect => new(RectPivotPosition, ZoomedSize);
    public override Rect FocusRect => TextureRect.ExpandedBy(15);

    public Material Material => texture.Material;
    public Texture Texture => Material.mainTexture;
    public TextureData Data => texture;
    public UIElement Owner => this;

    //Texture Data
    //public KeyFrameData EditData => internalFrameData;
    public KeyFrameData CurrentData => ParentCanvas.TimeLine.GetDataFor(this);

    //UIElement
    public UIElement Element => this;

    public void DrawElementInScroller(Rect inRect)
    {
        var mat = Material;
        TWidgets.DrawRotatedMaterial(
            TWidgets.RectFitted(inRect, TexCoords.size, Texture.Size(), TexCoordAnchor, StretchMode), Vector2.zero, 0,
            mat, TexCoords);

        Text.Anchor = TextAnchor.LowerLeft;
        TWidgets.DoTinyLabel(inRect, LayerTag);
        Text.Anchor = default;

        var varColor = Color.white;
        if (parentElement != null)
            varColor = Color.blue;

        if (!subParts.NullOrEmpty())
            varColor = Color.red;

        var bottomPart = inRect.BottomPartPixels(24).RightPartPixels(24);
        var buttonRow = new Verse.WidgetRow(bottomPart.x, bottomPart.y, UIDirection.RightThenUp, 24);
        if (buttonRow.ButtonIcon(TeleContent.LinkIcon,
                parentElement != null ? $"Linked To: {parentElement.LayerTag}" : null, iconColor: varColor))
        {
            var floatOptions = new List<FloatMenuOption>();
            foreach (TextureElement tex in ParentCanvas.ChildElements)
                if (tex != this)
                    floatOptions.Add(new FloatMenuOption(tex.LayerTag, delegate { LinkToParent(tex); }));
            Find.WindowStack.Add(new FloatMenu(floatOptions));
        }

        if (buttonRow.ButtonIcon(TeleContent.VisibilityOff, iconColor: Visibility ? TColor.White05 : Color.white))
            Visibility = !Visibility;
        //TWidgets.DoTinyLabel(inRect, $"{mat.mainTexture.name}\n{mat.shader.name}\n{RectSimple(texCoords ?? default)}\n{pivotPoint}\n{mat.mainTextureOffset}\n{mat.mainTextureScale}");
    }

    public void UpdateBuffer()
    {
        if (IsAtKeyFrame) CurrentData.UpdateBuffer(ValueBuffer);
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
        TPosition = pos ?? CurrentData.TPosition;
        TRotation = rot ?? CurrentData.TRotation;
        TSize = size ?? CurrentData.TSize;
        PivotPoint = pivot ?? PivotPoint;
        ParentCanvas.TimeLine.UpdateKeyframeFor(this, CurrentData);
        TexCoords = texture.TexCoordReference;
        UpdateBuffer();
    }

    public void SetTRSP_FromScreenSpace(Vector2? pos = null, float? rot = null, Vector2? size = null,
        Vector2? pivot = null)
    {
        SetTRSP_Direct(ParentCanvas.MouseOnCanvas);
    }

    public void Reset()
    {
        TSize = Vector2.one;
        TRotation = 0;
        PivotPoint = Vector2.zero;
        TexCoords = texture.TexCoordReference;
        ParentCanvas.TimeLine.UpdateKeyframeFor(this, CurrentData);
        UpdateBuffer();
    }

    public void Recenter()
    {
        TPosition = Vector2.zero;
        ParentCanvas.TimeLine.UpdateKeyframeFor(this, CurrentData);
        UpdateBuffer();
    }

    public void LinkToParent(TextureElement newParent)
    {
        newParent.SubParts.Add(this);
        parentElement = newParent;
        if (subParts.Contains(newParent))
            subParts.Remove(newParent);
        if (newParent.parentElement == this)
            newParent.parentElement = null;
    }

    public void UnlinkFromParent()
    {
        if (parentElement != null)
        {
            parentElement.SubParts.Remove(this);
            parentElement = null;
        }
    }

    protected override void Notify_RemovedFromParent(UIElement parent)
    {
        UnlinkFromParent();
        if (!subParts.NullOrEmpty())
            for (var i = subParts.Count - 1; i >= 0; i--)
            {
                var part = subParts[i];
                part.Notify_RemovedFromParent(parent);
            }
    }

    protected override void HandleEvent_Custom(Event ev, bool inContext)
    {
        if (!IsSelected) return;

        var mv = ev.mousePosition;
        var dist = mv.DistanceToRect(TextureRect);
        var pivotDist = Vector2.Distance(mv, RenderPivot);

        ManiMode = ManipulationMode.None;
        if (pivotDist <= 8)
            ManiMode = ManipulationMode.PivotDrag;
        else if (TextureRect.Contains(mv)) ManiMode = ManipulationMode.Move;

        if (dist is > 0 and < 5) ManiMode = ManipulationMode.Resize;

        if (dist is > 5 and < 15) ManiMode = ManipulationMode.Rotate;

        if (ev.type == EventType.MouseDown)
            if (ev.button == 0)
            {
                lockedInMode ??= ManiMode;
                oldKF ??= CurrentData;
                oldPivot ??= PivotPoint;

                if (ManiModeFlag) //Update Local
                    //LocalData = oldKF.Value;
                    UIEventHandler.StartFocusForced(this);
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
                    dragDiff /= TSizeFactor;
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
                    var newRot =
                        oldKF.Value.TRotation + Vector2.SignedAngle(vec1, vec2); //Vector2.SignedAngle(vec1, vec2);
                    TRotation = newRot;
                    break;
            }

            if (ManiModeFlag) //Update KeyFrame
                UpdateBuffer();
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
        if (!IsSelected) yield break;
        yield return new FloatMenuOption("Reset", Reset);
        yield return new FloatMenuOption("Delete", delegate { ParentCanvas.RemoveElement(this); });
        yield return new FloatMenuOption("Center", Recenter);
    }

    protected override void DrawContentsBeforeRelations(Rect inRect)
    {
        if (!Visibility) return;

        TWidgets.DrawRotatedMaterial(
            TWidgets.RectFitted(TextureRect, TexCoords.size, Texture.Size(), TexCoordAnchor, StretchMode), RenderPivot,
            TRotation, Material, TexCoords);

        if (TRotation != 0 && IsSelected)
        {
            var matrix = GUI.matrix;
            Verse.UI.RotateAroundPivot(TRotation, TextureOverlayRect.center);
            TWidgets.DrawBoxHighlight(TextureOverlayRect);
            GUI.matrix = matrix;
        }

        if (ShowTexCoordGhost)
        {
            //Widgets.BeginGroup(TextureRect);
            GUI.color = TColor.White05;
            TWidgets.DrawRotatedMaterial(TextureRect, Vector2.zero, 0, Material, new Rect(0, 0, 1, 1));
            GUI.color = Color.white;

            Verse.Widgets.BeginGroup(TextureRect);
            TWidgets.DrawBox(TWidgets.TexCoordsToRect(TextureRect, TexCoords), TColor.NiceBlue, 1);
            Verse.Widgets.EndGroup();
        }
    }

    internal static void DrawOverlay(TextureElement te)
    {
        Verse.Widgets.BeginGroup(te.Parent.InRect);
        {
            TWidgets.DrawBox(te.TextureRect, te.BorderColor, 1);

            //Draw Pivot
            GUI.color = te.ManiMode == ManipulationMode.PivotDrag ? Color.red : Color.white;
            Verse.Widgets.DrawTextureFitted(te.RenderPivot.RectOnPos(new Vector2(24, 24)), TeleContent.PivotPoint, 1,
                Vector2.one, new Rect(0, 0, 1, 1), te.TRotation);
            GUI.color = Color.white;
        }
        Verse.Widgets.EndGroup();
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