using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace TeleCore.Unsorted;

public interface IKeyFramedElement
{
    //public KeyFrameData EditData { get; }
    public KeyFrameData CurrentData { get; }
    public UIElement Owner { get; }
}

internal class TimeLineControl : UIElement
{
    //
    private const int _PixelsPerTick = 4;

    private const int _TopSize = 40;
    private const int _LeftSize = 125;
    private const int _ElementSize = 25;
    private const int _TimeLineContract = 10;

    private static int EventFlagCounter;
    private readonly FloatRange zoomRange = new(1f, 8f);
    private IntRange _tempReplayBounds;

    //Copy & Paste
    private (IKeyFramedElement, KeyFrame)? copiedKeyFrame;
    private KeyFrame? currentDraggedKF;

    //TimeLineSettings
    private int currentFrameInt;
    private (IKeyFramedElement, KeyFrame)? draggedKeyframe;

    private Vector2 elementScrollPos = Vector2.zero;
    private bool isPaused = true;

    private KeyFrame? oldKF;

    //
    private (IKeyFramedElement, KeyFrame)? selectedKeyframe;
    private Vector2 timeLineScrollPos = Vector2.zero;

    //UI
    private float zoomFactor = 1f;

    public TimeLineControl() : base(UIElementMode.Static)
    {
        TFind.TickManager.RegisterUITickAction(delegate
        {
            if (isPaused || !HasAnimation) return;
            if (CurrentFrame >= ReplayBounds.max)
            {
                CurrentFrame = ReplayBounds.min;
                return;
            }

            CurrentFrame++;
        });
    }

    //Animation Data
    public bool HasAnimation => Canvas.AnimationData.CurrentAnimations.Any();
    public AnimationPartValue CurrentPart => Canvas.CurrentAnimationPart;
    public int AnimationLength => CurrentPart.frames;
    public Dictionary<IKeyFramedElement, Dictionary<int, KeyFrame>> AnimationPartFrames => CurrentPart.InternalFrames;

    //Canvas Elements
    public List<UIElement> Elements => Canvas.ChildElements;
    public IEnumerable<IKeyFramedElement> KeyFramedElements => Canvas.ChildElements.Select(t => (IKeyFramedElement)t);

    private Rect LeftRect => InRect.LeftPartPixels(_LeftSize);
    private Rect RightRect => InRect.RightPartPixels(InRect.width - _LeftSize);

    private Rect TopLeft => LeftRect.TopPartPixels(_TopSize);
    private Rect BotLeft => LeftRect.BottomPartPixels(InRect.height - _TopSize);

    private Rect TopRight => RightRect.TopPartPixels(_TopSize);
    private Rect BotRight => RightRect.BottomPartPixels(InRect.height - _TopSize);

    private float TimeSelectorPctPos => CurrentFrame / (float)AnimationLength;

    private float CurrentZoom
    {
        get => zoomFactor;
        set => zoomFactor = Mathf.Clamp(value, zoomRange.min, zoomRange.max);
    }

    private float CurrentFrameXOffset => CurrentFrame * PixelPerTickAdjusted;
    private float PixelPerTickAdjusted => _PixelsPerTick * CurrentZoom;
    private float TimeLineLength => AnimationLength * PixelPerTickAdjusted;

    private IntRange ReplayBounds
    {
        get => CurrentPart.ReplayBounds;
        set => CurrentPart.ReplayBounds = value;
    }

    public int CurrentFrame
    {
        get => currentFrameInt;
        private set => currentFrameInt = Mathf.Clamp(value, 0, AnimationLength);
    }

    public TextureCanvas Canvas { get; set; }
    public IKeyFramedElement SelectedElement => Canvas.ActiveTexture;

    public void Reset()
    {
        isPaused = true;
    }

    //KeyFrames
    //Adding
    public void SetKeyFrameFor(IKeyFramedElement element)
    {
        UpdateKeyframeFor(element, GetDataFor(element));
    }

    public void SetEventFlag()
    {
        if (CurrentPart.InternalEventFlags.ContainsKey(CurrentFrame)) return;
        CurrentPart.InternalEventFlags.Add(CurrentFrame,
            new AnimationActionEventFlag(CurrentFrame, $"newFlag_{EventFlagCounter++}"));
    }

    //Removing - Remove known keyframe at current frame
    public void RemoveKeyFrameFor(IKeyFramedElement element)
    {
        if (AnimationPartFrames[element].ContainsKey(CurrentFrame)) AnimationPartFrames[element].Remove(CurrentFrame);
    }

    //Updating - Set new KeyFrameData at current frame for element
    public void UpdateKeyframeFor(IKeyFramedElement element, KeyFrameData data)
    {
        var updatedFrame = new KeyFrame(data, CurrentFrame);
        //If frame exists, set new keyframe with new data at frame
        if (AnimationPartFrames[element].ContainsKey(CurrentFrame))
        {
            AnimationPartFrames[element][CurrentFrame] = updatedFrame;
            return;
        }

        //Set new keyframe at current frame
        AnimationPartFrames[element].Add(CurrentFrame, updatedFrame);
    }

    public bool IsAtKeyFrame(IKeyFramedElement element, out KeyFrame frameAt)
    {
        return AnimationPartFrames[element].TryGetValue(CurrentFrame, out frameAt);
    }

    //
    public bool GetKeyFrames(IKeyFramedElement element, out KeyFrame? frame1, out KeyFrame? frame2, out float dist)
    {
        frame1 = frame2 = null;
        var frames = AnimationPartFrames[element];
        var framesMin = frames.Where(t => t.Key <= CurrentFrame);
        var framesMax = frames.Where(t => t.Key >= CurrentFrame);
        if (framesMin.TryMaxBy(t => t.Key, out var value1))
            frame1 = value1.Value;

        if (framesMax.TryMinBy(t => t.Key, out var value2))
            frame2 = value2.Value;
        dist = Mathf.InverseLerp(frame1?.Frame ?? 0, frame2?.Frame ?? 0, CurrentFrame);

        return frame1 != null && frame2 != null;
    }

    public KeyFrame GetKeyFrame(IKeyFramedElement element)
    {
        if (IsAtKeyFrame(element, out var frame)) return frame;
        if (GetKeyFrames(element, out var frame1, out var frame2, out var lerpVal))
            return new KeyFrame(frame1.Value.Data.Interpolated(frame2.Value.Data, lerpVal), CurrentFrame);
        if (frame1.HasValue)
            return frame1.Value;
        if (frame2.HasValue)
            return frame2.Value;
        return KeyFrame.Invalid;
    }

    public KeyFrameData GetDataFor(IKeyFramedElement element)
    {
        //if (!AnimationPartFrames.ContainsKey(element)) return element.KeyFrameData;
        if (IsAtKeyFrame(element, out var frame)) return frame.Data;
        if (GetKeyFrames(element, out var frame1, out var frame2, out var lerpVal))
            return frame1.Value.Data.Interpolated(frame2.Value.Data, lerpVal);

        if (frame1.HasValue)
            return frame1.Value.Data;
        if (frame2.HasValue)
            return frame2.Value.Data;

        return new KeyFrameData(Vector2.zero, 0, Vector2.one);
    }

    protected override void HandleEvent_Custom(Event ev, bool inContext)
    {
        base.HandleEvent_Custom(ev);

        if (ev.type == EventType.KeyDown)
        {
            if (ev.keyCode == KeyCode.LeftArrow)
                CurrentFrame--;
            if (ev.keyCode == KeyCode.RightArrow)
                CurrentFrame++;
        }

        var holdingLShift = Input.GetKey(KeyCode.LeftShift);
        var holdingCtrl = Input.GetKey(KeyCode.LeftControl);
        if (holdingCtrl && Input.GetKeyDown(KeyCode.C))
            if (selectedKeyframe.HasValue)
            {
                var selKF = selectedKeyframe.Value;
                copiedKeyFrame = (selKF.Item1, selKF.Item2);
            }

        if (holdingCtrl && Input.GetKeyDown(KeyCode.V))
            if (copiedKeyFrame.HasValue)
                UpdateKeyframeFor(copiedKeyFrame?.Item1, copiedKeyFrame.Value.Item2.Data);

        if (holdingLShift && ev.isScrollWheel && RightRect.Contains(ev.mousePosition))
        {
            CurrentZoom += Input.mouseScrollDelta.y / 10f;
            ev.Use();
        }

        if (Input.GetKeyDown(KeyCode.Delete))
            if (selectedKeyframe.HasValue)
                CurrentPart.InternalFrames[selectedKeyframe.Value.Item1].Remove(selectedKeyframe.Value.Item2.Frame);

        /*
        if (ev.isScrollWheel)
        {
            CurrentZoom += Input.mouseScrollDelta.y/10f;
        }
        if (ev.type == EventType.MouseDown && ev.button == 2)
        {
            CurrentZoom = 1f;
        }
        */
    }

    protected override void DrawContentsBeforeRelations(Rect inRect)
    {
        if (!HasAnimation)
            /*
            var workingRect = inRect.LeftPartPixels(260);
            TWidgets.DrawColoredBox(workingRect, TColor.BGDarker, TColor.MenuSectionBGBorderColor, 1);
            workingRect = workingRect.ContractedBy(5);
            Listing_Standard listing = new Listing_Standard();
            listing.Begin(workingRect.LeftPartPixels(250));

            if (!Canvas.Initialized)
            {
                GUI.color = Color.red;
                listing.Label("Animation Set Not Ready");
                GUI.color = Color.white;
                listing.End();
                return;
            }

            listing.Label("New Animation Settings");
            listing.GapLine();
            listing.TextFieldLabeled("Tag:" ,ref animationPartName, anchor:TextAnchor.MiddleLeft);
            listing.TextFieldNumericLabeled("Length:", ref timeSeconds, ref _timeSecondsBuffer, anchor:TextAnchor.MiddleLeft);

            listing.Gap();
            if (listing.ButtonText("Add"))
            {
                Canvas.AnimationDataDef.Notify_CreateNewAnimationPart(animationPartName, timeSeconds);
                animationPartName = "Animation_Tag";
                timeSeconds = 5;
                _timeSecondsBuffer = "5";
            }

            listing.End();
            */
            return;

        TimeControlButtons(TopRect.RightPartPixels(TopRect.width - _LeftSize));

        //Element List Scroller
        var elementListCount = Elements.Count;
        var elementListViewRect = new Rect(BotLeft.x, BotLeft.y, BotLeft.width, elementListCount * _ElementSize);

        //Time Line Scroller
        var timeLineSelectorRect = new Rect(TopRight.x, TopRight.y, TimeLineLength, TopRight.height);
        var timelineViewRect =
            new Rect(TopRight.x, TopRight.y, TimeLineLength, RightRect.height).ExpandedBy(_TimeLineContract, 0);
        var timelineBotViewRect = new Rect(BotRight.x, BotRight.y, TimeLineLength, elementListViewRect.height);

        //
        float curY = 0;
        Widgets.BeginScrollView(BotLeft, ref elementScrollPos, elementListViewRect, false);
        {
            curY = elementListViewRect.y;
            foreach (var element in KeyFramedElements)
            {
                var left = new Rect(BotLeft.x, curY, BotLeft.width, _ElementSize);
                ElementListing(left, element);
                curY += _ElementSize;
            }
        }
        Widgets.EndScrollView();

        //
        Widgets.DrawBoxSolid(RightRect, TColor.BGDarker);
        Widgets.ScrollHorizontal(RightRect, ref timeLineScrollPos, timelineViewRect);
        Widgets.BeginScrollView(RightRect, ref timeLineScrollPos, timelineViewRect, false);
        {
            DrawTimeSelector(timeLineSelectorRect, timelineViewRect.height);
            DrawEventFlags(timeLineSelectorRect, timelineViewRect.height);
        }
        Widgets.EndScrollView();

        timeLineScrollPos = new Vector2(timeLineScrollPos.x, elementScrollPos.y);
        GUI.BeginScrollView(BotRight, timeLineScrollPos, timelineBotViewRect, GUIStyle.none, GUIStyle.none);
        {
            curY = timelineBotViewRect.y;
            foreach (var element in KeyFramedElements)
            {
                var right =
                    new Rect(timelineBotViewRect.x, curY, TimeLineLength, _ElementSize).ContractedBy(_TimeLineContract,
                        0);
                ElementTimeLine(right, element);
                curY += _ElementSize;
            }
        }
        GUI.EndScrollView();

        TWidgets.DrawBox(RightRect, TColor.MenuSectionBGBorderColor, 1);
    }

    private void DrawEventFlags(Rect rect, float height)
    {
        if (CurrentPart?.InternalEventFlags == null) return;

        foreach (var eventFlag in CurrentPart.InternalEventFlags)
        {
            var flagValue = eventFlag.Value;
            var valX = rect.x + eventFlag.Key * PixelPerTickAdjusted;
            var selectorRect = new Rect(valX - 12, rect.y, 25, 25);

            GUI.color = TColor.Orange;
            Widgets.DrawLineVertical(valX, selectorRect.yMax - 5, height);
            GUI.color = Color.white;

            var textSize = Text.CalcSize(flagValue.EventTag);
            TWidgets.DoTinyLabel(new Rect(valX, selectorRect.yMin - 5, textSize.x, 0), flagValue.EventTag);
        }
    }

    private void DrawTimeSelector(Rect timeBar, float totalHeight)
    {
        GUI.color = TColor.MenuSectionBGBorderColor;
        Widgets.DrawLineHorizontal(timeBar.x, timeBar.yMax, timeBar.width);
        GUI.color = Color.white;

        //Draw Selector
        _tempReplayBounds = ReplayBounds;
        DrawTimeSelectorCustom(timeBar, GetHashCode(), ref _tempReplayBounds, ref currentFrameInt, totalHeight, 0,
            AnimationLength);
        ReplayBounds = _tempReplayBounds;

        //Draw Tick Lines
        var curX = timeBar.x;
        for (var i = 0; i <= AnimationLength; i++)
        {
            var bigOne = i % 60 == 0;
            var length = bigOne ? 8 : 4;
            var pos = new Vector2(curX, timeBar.yMax - (length + 1));
            Widgets.DrawLineVertical(pos.x, pos.y, length);
            if (bigOne)
            {
                var label = $"{i / 60}s";
                pos -= new Vector2(0, 4);
                TWidgets.DoTinyLabel(pos.RectOnPos(Text.CalcSize(label)), label);
            }

            curX += PixelPerTickAdjusted;
        }
    }

    private void DrawTimeSelectorCustom(Rect rect, int id, ref IntRange range, ref int value, float verticalHeight,
        int min = 0, int max = 100, int minWidth = 0)
    {
        //Custom Line
        /*
        GUI.color = Widgets.RangeControlTextColor;
        Rect barRect = new Rect(rect.x, rect.y + 8f, rect.width, 2f);
        GUI.DrawTexture(barRect, BaseContent.WhiteTex);
        GUI.color = Color.white;
        */

        //Selector Positions
        var leftX = rect.x +
                    range.min *
                    PixelPerTickAdjusted; //marginRect.width * (float)(range.min - min) / (float)(max - min);
        var rightX =
            rect.x + range.max *
            PixelPerTickAdjusted; //marginRect.width * (float)(range.max - min) / (float)(max - min);
        var valX = rect.x +
                   value * PixelPerTickAdjusted; //marginRect.width * (float) (value - min) / (float) (max - min);
        var rangeLPos = new Rect(leftX, rect.y, 16f, 16f);
        var rangeRPos = new Rect(rightX - 16f, rect.y, 16f, 16f);
        var rangeLBar = new Rect(rangeLPos.position, new Vector2(rangeLPos.width, rect.height));
        var rangeRBar = new Rect(rangeRPos.position, new Vector2(rangeRPos.width, rect.height));
        var selectorRect = new Rect(valX - 12, rect.y, 25, 25);
        GUI.DrawTexture(rangeLPos, TeleContent.TimeSelRangeL);
        GUI.DrawTexture(rangeRPos, TeleContent.TimeSelRangeR);

        Widgets.DrawLineVertical(valX, selectorRect.yMax - 5, verticalHeight);

        if (Widgets.curDragEnd == Widgets.RangeEnd.None && (Widgets.draggingId == id || Mouse.IsOver(selectorRect)))
            GUI.color = Color.cyan;

        GUI.DrawTexture(selectorRect, TeleContent.TimeSelMarker);
        GUI.color = Color.white;

        //valPos = new Rect(valPos.x + 8, valPos.y, valPos.width, valPos.height);
        //Widgets.Label(valPos, $"{CurrentFrame}");

        if (Widgets.curDragEnd == Widgets.RangeEnd.Min || Mouse.IsOver(rangeLBar))
            Widgets.DrawHighlight(rangeLBar);
        if (Widgets.curDragEnd == Widgets.RangeEnd.Max || Mouse.IsOver(rangeRBar))
            Widgets.DrawHighlight(rangeRBar);

        if (!UIEventHandler.IsFocused(this)) return;

        var mouseUp = Event.current.type == EventType.MouseUp;
        if (mouseUp || Event.current.rawType == EventType.MouseDown)
        {
            if (mouseUp)
                Widgets.curDragEnd = Widgets.RangeEnd.None;
            Widgets.draggingId = 0;
            SoundDefOf.DragSlider.PlayOneShotOnCamera();
        }

        var flag = false;
        if (Mouse.IsOver(rect) || Widgets.draggingId == id)
        {
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && id != Widgets.draggingId)
            {
                Widgets.draggingId = id;
                var x = Event.current.mousePosition.x;
                if (x >= rangeLPos.xMin && x <= rangeLPos.xMax)
                    Widgets.curDragEnd = Widgets.RangeEnd.Min;
                else if (x >= rangeRPos.xMin && x <= rangeRPos.xMax)
                    Widgets.curDragEnd = Widgets.RangeEnd.Max;
                else
                    Widgets.curDragEnd = Widgets.RangeEnd.None;
                /*
                    float num3 = Mathf.Abs(x - rangeLPos.xMax);
                    float num4 = Mathf.Abs(x - (rangeRPos.x - 16f));
                    Widgets.curDragEnd = ((num3 < num4) ? Widgets.RangeEnd.Min : Widgets.RangeEnd.Max);
                    */
                flag = true;
                Event.current.Use();
                SoundDefOf.DragSlider.PlayOneShotOnCamera();
            }

            if (flag || Event.current.type == EventType.MouseDrag)
            {
                var num5 = Mathf.RoundToInt(
                    Mathf.Clamp((Event.current.mousePosition.x - rect.x) / rect.width * (max - min) + min, min, max));

                //Value Selection
                if (Widgets.curDragEnd == Widgets.RangeEnd.None)
                {
                    var newSliderVal =
                        Mathf.RoundToInt(Mathf.Clamp(
                            (Event.current.mousePosition.x - rect.x) / rect.width * (max - min) + min, min, max));
                    value = Mathf.Clamp(newSliderVal, min, max);
                }

                //Range selection
                if (Widgets.curDragEnd == Widgets.RangeEnd.Min)
                {
                    if (num5 != range.min)
                    {
                        range.min = num5;
                        if (range.min > max - minWidth) range.min = max - minWidth;
                        var num6 = Mathf.Max(min, range.min + minWidth);
                        if (range.max < num6) range.max = num6;
                    }
                }
                else if (Widgets.curDragEnd == Widgets.RangeEnd.Max && num5 != range.max)
                {
                    range.max = num5;
                    if (range.max < min + minWidth) range.max = min + minWidth;
                    var num7 = Mathf.Min(max, range.max - minWidth);
                    if (range.min > num7) range.min = num7;
                }

                Widgets.CheckPlayDragSliderSound();
                Event.current.Use();
            }
        }
    }

    private void ElementListing(Rect leftRect, IKeyFramedElement element)
    {
        TWidgets.DrawBox(leftRect, SelectedElement == element ? Color.cyan : TColor.White05, 1);
        Widgets.Label(leftRect, $"{((TextureElement)element).LayerTag}");
        if (Widgets.ButtonImage(leftRect.RightPartPixels(20), TeleContent.DeleteX))
            AnimationPartFrames[element].Clear();
    }

    private void TryStartOrDragKeyframe(IKeyFramedElement element, KeyFrame keyFrame, Rect selRect)
    {
        //
        var ev = Event.current;
        if (ev.type == EventType.MouseDown && Mouse.IsOver(selRect))
        {
            //Left Click
            if (ev.button == 0)
            {
                oldKF ??= keyFrame;
                currentDraggedKF ??= keyFrame;
                draggedKeyframe ??= (element, keyFrame);
                SetSelectedKeyFrame(element, keyFrame);
            }

            //Right Click
            if (ev.button == 1)
                Find.WindowStack.Add(new FloatMenu(new List<FloatMenuOption>
                {
                    new("Delete", () => { CurrentPart.InternalFrames[element].Remove(keyFrame.Frame); })
                }));
        }

        if (ev.type == EventType.MouseDrag)
            //
            if (oldKF.HasValue)
            {
                var tickDiff = oldKF.Value.Frame + Mathf.RoundToInt(CurrentDragDiff.x / PixelPerTickAdjusted);
                currentDraggedKF = new KeyFrame(oldKF.Value.Data, Mathf.Clamp(tickDiff, 0, AnimationLength));
            }

        if (ev.type == EventType.MouseUp)
        {
            if (currentDraggedKF.HasValue)
                if (currentDraggedKF.Value.Frame != oldKF.Value.Frame)
                {
                    //Drop dragged and replace with old KeyFrame
                    CurrentPart.InternalFrames[draggedKeyframe.Value.Item1].Remove(oldKF.Value.Frame);
                    CurrentPart.InternalFrames[draggedKeyframe.Value.Item1]
                        .Add(currentDraggedKF.Value.Frame, currentDraggedKF.Value);
                    SetSelectedKeyFrame(draggedKeyframe.Value.Item1, currentDraggedKF.Value);
                }

            //Reset
            draggedKeyframe = null;
            oldKF = null;
            currentDraggedKF = null;
        }
    }

    private void SetSelectedKeyFrame(IKeyFramedElement element, KeyFrame keyFrame)
    {
        selectedKeyframe = (element, keyFrame);
    }

    private bool IsSelected(IKeyFramedElement element, KeyFrame keyFrame)
    {
        if (!selectedKeyframe.HasValue) return false;
        return selectedKeyframe.Value.Item2 == keyFrame && selectedKeyframe?.Item1 == element;
    }

    private void ElementTimeLine(Rect rightRect, IKeyFramedElement element)
    {
        Widgets.DrawHighlightIfMouseover(rightRect);
        GUI.color = SelectedElement == element ? Color.white : TColor.White025;
        var yPos = rightRect.y + rightRect.height / 2f;
        Widgets.DrawLineHorizontal(rightRect.x, yPos, rightRect.width);
        GUI.color = Color.white;

        var elements = AnimationPartFrames[element].Values.ToList();
        for (var i = 0; i < elements.Count; i++)
        {
            var keyFrame = elements[i];
            var rect =
                new Vector2(rightRect.x + keyFrame.Frame * PixelPerTickAdjusted, yPos).RectOnPos(new Vector2(20, 20));
            TooltipHandler.TipRegion(rect, $"Frame:\n{element.CurrentData}");

            //Dragger
            TryStartOrDragKeyframe(element, keyFrame, rect);

            if (draggedKeyframe.HasValue)
                if (draggedKeyframe.Value.Item1 == element && draggedKeyframe.Value.Item2 == keyFrame)
                {
                    GUI.color = TColor.NiceBlue;
                    var rect2 = new Vector2(rightRect.x + currentDraggedKF.Value.Frame * PixelPerTickAdjusted, yPos)
                        .RectOnPos(new Vector2(20, 20));
                    Widgets.DrawTextureFitted(rect2, TeleContent.KeyFrame, 1f);
                    GUI.color = Color.white;
                }

            var isSelected = IsSelected(element, keyFrame);
            if (isSelected)
                GUI.color = TColor.Blue;

            if (keyFrame.Equals(oldKF))
                GUI.color = TColor.White05;

            if (Mouse.IsOver(rect))
                GUI.color = TColor.NiceBlue;

            Widgets.DrawTextureFitted(rect, TeleContent.KeyFrame, 1f);
            if (isSelected) Widgets.DrawTextureFitted(rect, TeleContent.KeyFrameSelection, 1f);

            GUI.color = Color.white;
        }
    }

    private void TimeControlButtons(Rect topPart)
    {
        Widgets.BeginGroup(topPart);
        var row = new WidgetRow();
        if (row.ButtonIcon(TeleContent.PlayPause)) isPaused = !isPaused;

        //Add at current frame
        if (row.ButtonIcon(TeleContent.AddKeyFrame))
            foreach (var element in KeyFramedElements)
                SetKeyFrameFor(element);

        //Remove at current frame
        if (row.ButtonIcon(TeleContent.AddKeyFrame, backgroundColor: Color.red))
            foreach (var element in KeyFramedElements)
                RemoveKeyFrameFor(element);

        //Add Animation Event
        if (row.ButtonIcon(TeleContent.AddKeyFrame, backgroundColor: Color.yellow)) SetEventFlag();

        row.Gap(8);
        row.Slider(125, ref zoomFactor, zoomRange.min, zoomRange.max);

        if (Canvas.ActiveTexture != null)
        {
            //Canvas.ActiveTexture.SetTRSP_Direct(rot: Canvas.ActiveTexture.TRotation);
        }

        row.Gap(8);
        row.Label($"[{CurrentFrame}][{TMath.Round(CurrentFrame.TicksToSeconds(), 2)}s]");


        row.Init(0, Rect.height - 16, UIDirection.LeftThenUp);

        Widgets.EndGroup();
    }

    public void UpdateAllFramesFor(IKeyFramedElement element, ManipulationMode forMode, object byValue)
    {
        var elements = AnimationPartFrames[element].Values.ToList();
        foreach (var frame in elements)
        {
            if (CurrentFrame == frame.Frame) continue;
            var data = frame.Data;
            switch (forMode)
            {
                case ManipulationMode.Move:
                    data.TPosition += (Vector2)byValue;
                    break;
                case ManipulationMode.Resize:
                    data.TSize += (Vector2)byValue;
                    break;
                case ManipulationMode.Rotate:
                    data.TRotation += (float)byValue;
                    break;
                case ManipulationMode.PivotDrag:
                    data.PivotPoint += (Vector2)byValue;
                    break;
                default: return;
            }

            Update(element, data, frame.Frame);
        }
    }

    private void Update(IKeyFramedElement element, KeyFrameData data, int frame)
    {
        var updatedFrame = new KeyFrame(data, frame);
        if (AnimationPartFrames[element].ContainsKey(frame))
        {
            AnimationPartFrames[element][frame] = updatedFrame;
            return;
        }

        AnimationPartFrames[element].Add(frame, updatedFrame);
    }
}