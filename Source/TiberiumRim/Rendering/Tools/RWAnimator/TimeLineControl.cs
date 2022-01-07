using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace TiberiumRim
{
    public interface IKeyFramedElement
    {
        public KeyFrameData KeyFrameData { get; }
        public UIElement Owner { get; }
        void SetTRS(KeyFrameData? data);
    }

    public struct KeyFrameData
    {
        //TRS
        public float rotation;
        public Vector2 position, size;

        public float layerPos;

        public KeyFrameData(Vector2 pos, float rot, Vector2 size)
        {
            position = pos;
            rotation = rot;
            this.size = size;
            layerPos = 0;
        }

        public KeyFrameData Interpolated(KeyFrameData other, float val)
        {
            return new KeyFrameData(
                Vector2.Lerp(position, other.position, val),
                Mathf.Lerp(rotation, other.rotation, val),
                Vector2.Lerp(size, other.size, val));
        }
    }

    public struct KeyFrame
    {
        public KeyFrameData frameData;
        public int frameTick;

        public KeyFrame(KeyFrameData data, int tick)
        {
            frameData = data;
            frameTick = tick;
        }

        public float Second => frameTick.TicksToSeconds();
    }

    public class KeyFrameList
    {
        private KeyFrame[] keyFrames = Array.Empty<KeyFrame>();

        public KeyFrameList()
        {

        }

        public void AddKeyframe(KeyFrame newFrame)
        {
            int size = keyFrames.Length + 1;
            Array.Resize(ref keyFrames, size);
            keyFrames[size - 1] = newFrame;
        }
        public void RemoveKeyframe(KeyFrame newFrame)
        {
            int size = keyFrames.Length + 1;
            Array.Resize(ref keyFrames, size);
            keyFrames[size - 1] = newFrame;
        }

    }

    public class TimeControlTicker
    {
        private Stopwatch clock = new Stopwatch();

        private float realTimeToTickThrough;
        private bool isPaused = true;
        private Action actions;

        private int timeControlTicks;

        private float CurTimePerTick => 1f / (60f);
        public bool Paused => isPaused;
        public int CurrentTick => timeControlTicks;

        public TimeControlTicker()
        {
            TiberiumRoot.WindowAnimator_TimeControl = this;
        }

        public void Update()
        {
            if (Paused) return;
            float curTimePerTick = CurTimePerTick;
            if (Mathf.Abs(Time.deltaTime - curTimePerTick) < curTimePerTick * 0.1f)
            {
                realTimeToTickThrough += curTimePerTick;
            }
            else
            {
               realTimeToTickThrough += Time.deltaTime;
            }

            int num = 0;
            clock.Reset();
            clock.Start();
            while (realTimeToTickThrough > 0f && (float)num < 2)
            {
                DoSingleTick();
                realTimeToTickThrough -= curTimePerTick;
                num++;

                if (Paused || (float)clock.ElapsedMilliseconds > 1000f / 30f)
                {
                    break;
                }
            }
        }

        private void DoSingleTick()
        {
            timeControlTicks++;
            actions.Invoke();
        }

        public void TogglePlay()
        {
            isPaused = !isPaused;
        }
        public void AddTickAction(Action action)
        {
            actions += action;
        }
    }

    public class TimeLineControl : UIElement
    {
        //
        private const int _PixelsPerTick = 4;

        public readonly TimeControlTicker ticker;

        private int tickLength;
        private int currentFrameInt;
        private IntRange playTickRange;
        public Dictionary<IKeyFramedElement, Dictionary<int, KeyFrame>> framedElements = new();

        //UI
        private float zoomFactor = 1f;
        private FloatRange zoomRange = new FloatRange(0.25f, 2f);

        private Vector2 elementScrollPos = Vector2.zero;
        private Vector2 timeLineScrollPos = Vector2.zero;

        private float TimeSelectorPctPos => CurrentFrame / (float)tickLength;

        private float CurrentZoom
        {
            get => zoomFactor;
            set => zoomFactor = Mathf.Clamp(value, zoomRange.min, zoomRange.max);
        }

        public override UIElementMode UIMode => UIElementMode.Static;

        public List<UIElement> Elements => Canvas.ElementList;

        private float CurrentFrameXOffset => CurrentFrame * PixelPerTickAdjusted;
        private float PixelPerTickAdjusted => _PixelsPerTick * CurrentZoom;
        private float TimeLineLength => tickLength * PixelPerTickAdjusted;

        private IntRange PlayRange
        {
            get => playTickRange;
            set => playTickRange = value;
        }

        public int CurrentFrame
        {
            get => currentFrameInt;
            private set => currentFrameInt = Mathf.Clamp(value, 0, tickLength);
        }

        public TextureCanvas Canvas { get; set; }
        public IKeyFramedElement SelectedElement => Canvas.ActiveTexture;

        public bool IsAtKeyFrame(IKeyFramedElement element)
        {
            return framedElements[element].ContainsKey(CurrentFrame);
        }

        //This implies it is between two frames
        public bool GetKeyFrames(IKeyFramedElement element, out KeyFrame? frame1, out KeyFrame? frame2, out float dist)
        {
            frame1 = frame2 = null;
            var frames = framedElements[element];
            var framesMin = frames.Where(t => t.Key <= CurrentFrame);
            var framesMax = frames.Where(t => t.Key >= CurrentFrame);
            if (framesMin.TryMaxBy(t=> t.Key, out var value1))
                frame1 = value1.Value;

            if (framesMax.TryMinBy(t=> t.Key, out var value2))
                frame2 = value2.Value;
            dist = Mathf.InverseLerp(frame1?.frameTick ?? 0, frame2?.frameTick ?? 0, CurrentFrame);
            return frame1 != null && frame2 != null;
        }

        public KeyFrameData? GetDataFor(IKeyFramedElement element)
        {
            if (!framedElements.ContainsKey(element)) return null;
            if (IsAtKeyFrame(element)) return framedElements[element][CurrentFrame].frameData;
            if (GetKeyFrames(element, out var frame1, out var frame2, out var lerpVal))
                return frame1.Value.frameData.Interpolated(frame2.Value.frameData, lerpVal);

            return frame1?.frameData ?? frame2?.frameData;
        }

        public TimeLineControl()
        {
            ticker = new TimeControlTicker();
            ticker.AddTickAction(delegate
            {
                if (CurrentFrame >= PlayRange.max)
                {
                    CurrentFrame = PlayRange.min;
                    return;
                }
                CurrentFrame++;
            });

            //
            tickLength = 10f.SecondsToTicks();
            playTickRange = new IntRange(0, tickLength);
        }

        public void Notify_NewElement(IKeyFramedElement element)
        {
            framedElements.Add(element, new Dictionary<int, KeyFrame>());
        }

        public void Notify_RemovedElement(UIElement element)
        {
            if (element is not IKeyFramedElement framedElement) return;
            if (framedElements.ContainsKey(framedElement))
            {
                framedElements.Remove(framedElement);
            }
        }

        public void UpdateKeyframeFor(IKeyFramedElement element, KeyFrameData data)
        {
            if (framedElements[element].TryGetValue(CurrentFrame, out _))
            {
                framedElements[element][CurrentFrame] = new KeyFrame(data, CurrentFrame);
                return;
            }
            framedElements[element].Add(CurrentFrame, new KeyFrame(element.KeyFrameData, CurrentFrame));
        }

        public void SetKeyFrameFor(IKeyFramedElement element)
        {
            var data = GetDataFor(element);
            if (data != null)
                element.SetTRS(data);
            if (framedElements[element].ContainsKey(CurrentFrame))
            {
                framedElements[element][CurrentFrame] = new KeyFrame(element.KeyFrameData, CurrentFrame);
                return;
            }

            framedElements[element].Add(CurrentFrame, new KeyFrame(element.KeyFrameData, CurrentFrame));
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

        protected override void DrawContents(Rect inRect)
        {
            Rect topPart = inRect.TopPartPixels(30);
            Rect botPart = inRect.BottomPartPixels(inRect.height - topPart.height);

            Rect leftPart = inRect.LeftPartPixels(100);
            Rect rightPart = inRect.RightPartPixels(inRect.width - leftPart.width);

            Rect elementListRect = new Rect(0, botPart.y, leftPart.width, botPart.height);
            Rect timeLineElementListRect = new Rect(leftPart.width, botPart.y, rightPart.width, botPart.height);

            //

            Rect elementScrollList = new Rect(elementListRect.x, elementListRect.y, elementListRect.width, framedElements.Count * 20f);
            Rect timeLineElementScrollRect = new Rect(timeLineElementListRect.x, timeLineElementListRect.y, TimeLineLength, Mathf.Max(rightPart.height, elementScrollList.height));

            Rect timeLineFullScrollRect = new Rect(rightPart.x, rightPart.y, TimeLineLength, timeLineElementScrollRect.height).ExpandedBy(10, 0);

            //
            Rect playRect = new Rect(leftPart.xMax - 16, leftPart.y, 16, 16);
            Rect topBar = new Rect(rightPart.x, TopRect.y, rightPart.width, TopRect.height);
            TimeControlButtons(playRect, topBar);

            //Scrolling through left element list
            Widgets.BeginScrollView(elementListRect, ref elementScrollPos, elementScrollList, false);
            float curY = elementListRect.y;
            foreach (var element in framedElements.Keys)
            {
                Rect left = new Rect(leftPart.x, curY, leftPart.width, 20);
                ElementListing(left, element);
                curY += 20;
            }
            Widgets.EndScrollView();

            //Scrolling on the right timeline
            Widgets.ScrollHorizontal(rightPart, ref timeLineScrollPos, timeLineFullScrollRect);
            Widgets.BeginScrollView(rightPart, ref timeLineScrollPos, timeLineFullScrollRect, false);
            //
            Widgets.DrawBoxSolid(timeLineFullScrollRect, TRMats.BGDarker);

            //
            GUI.BeginScrollView(timeLineElementListRect, elementScrollPos, timeLineElementScrollRect, GUIStyle.none, GUIStyle.none);
            if (!framedElements.NullOrEmpty())
            {
                curY = timeLineElementListRect.y;
                foreach (var element in framedElements.Keys)
                {
                    Rect right = new Rect(rightPart.x, curY, rightPart.width, 20);
                    ElementTimeLine(right, element);
                    curY += 20;
                }
            }
            GUI.EndScrollView();

            //
            timeLineFullScrollRect = timeLineFullScrollRect.ContractedBy(10, 0);
            DrawTimeSelector(timeLineFullScrollRect);
            Widgets.EndScrollView();

            TRWidgets.DrawBox(rightPart, TRMats.MenuSectionBGBorderColor, 1);
        }

        private void DrawTimeSelector(Rect rect)
        {
            //TopPart
            Rect timeBar = rect.TopPartPixels(30);

            //Draw Global
            //TopPart Line
            GUI.color = TRMats.MenuSectionBGBorderColor;
            Widgets.DrawLineHorizontal(timeBar.x, timeBar.yMax, timeBar.width);
            GUI.color = Color.white;

            //Draw Selector
            DrawTimeSelectorCustom(rect, GetHashCode(), ref playTickRange, ref currentFrameInt, 0, tickLength);

            //TRWidgets.SliderCustom(rect, this.GetHashCode(), ref currentFrameInt, 0, tickLength, TiberiumContent.TimeSelMarker, Color.cyan);
            //TRWidgets.SliderRangeCustom(rect, GetHashCode()+1, ref playTickRange, 0, tickLength, leftTexture:TiberiumContent.TimeSelRangeL, rightTexture:TiberiumContent.TimeSelRangeR);
            //CurrentFrame = (int)Widgets.HorizontalSlider(rect, CurrentFrame, 0, tickLength, roundTo: 1);

            //Draw Tick Lines
            GUI.BeginGroup(timeBar);
            timeBar = timeBar.AtZero();
            float curX = 0;
            for (int i = 0; i < tickLength; i++)
            {
                bool bigOne = i % 60 == 0;
                var length = bigOne ? 8 : 4;
                var pos = new Vector2(curX, timeBar.yMax - (length + 1));
                Widgets.DrawLineVertical(pos.x, pos.y, length);
                if (bigOne)
                {
                    var label = $"{i / 60}s";
                    pos -= new Vector2(0, 4);
                    TRWidgets.DoTinyLabel(pos.RectOnPos(Text.CalcSize(label)), label);
                }
                curX += PixelPerTickAdjusted;
            }

            GUI.EndGroup();
        }

        private void DrawTimeSelectorCustom(Rect rect, int id, ref IntRange range, ref int value, int min = 0, int max = 100, int minWidth = 0)
        {
            //Custom Line
            GUI.color = Widgets.RangeControlTextColor;
            Rect barRect = new Rect(rect.x, rect.y + 8f, rect.width, 2f);
            GUI.DrawTexture(barRect, BaseContent.WhiteTex);
            GUI.color = Color.white;

            //Selector Positions
            float leftX = rect.x + (range.min * PixelPerTickAdjusted); //marginRect.width * (float)(range.min - min) / (float)(max - min);
            float rightX = rect.x + (range.max * PixelPerTickAdjusted); //marginRect.width * (float)(range.max - min) / (float)(max - min);
            float valX = rect.x + (value * PixelPerTickAdjusted); //marginRect.width * (float) (value - min) / (float) (max - min);
            Rect rangeLPos = new Rect(leftX, rect.y, 16f, 16f);
            Rect rangeRPos = new Rect(rightX - 16f, rect.y, 16f, 16f);
            Rect valPos = new Rect(valX-12, rect.y, 25, 25);
            GUI.DrawTexture(rangeLPos,  TiberiumContent.TimeSelRangeL);
            GUI.DrawTexture(rangeRPos, TiberiumContent.TimeSelRangeR);

            Widgets.DrawLineVertical(valX, valPos.yMax, rect.height);
            GUI.DrawTexture(valPos, TiberiumContent.TimeSelMarker);

            valPos = new Rect(valPos.x + 8, valPos.y, valPos.width, valPos.height);
            Widgets.Label(valPos, $"{CurrentFrame}");

            var mouseUp = Event.current.type == EventType.MouseUp;
            if ( (mouseUp || Event.current.rawType == EventType.MouseDown))
            {
                if (mouseUp)
                    Widgets.curDragEnd = Widgets.RangeEnd.None;
                Widgets.draggingId = 0;
                SoundDefOf.DragSlider.PlayOneShotOnCamera(null);
            }
            bool flag = false;
            if (Mouse.IsOver(rect) || Widgets.draggingId == id)
            {
                if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && id != Widgets.draggingId)
                {
                    Widgets.draggingId = id;
                    float x = Event.current.mousePosition.x;
                    if (x >= rangeLPos.xMin && x <= rangeLPos.xMax)
                    {
                        Widgets.curDragEnd = Widgets.RangeEnd.Min;
                    }
                    else if (x >= rangeRPos.xMin && x <= rangeRPos.xMax)
                    {
                        Widgets.curDragEnd = Widgets.RangeEnd.Max;
                    }
                    else
                    {
                        Widgets.curDragEnd = Widgets.RangeEnd.None;
                        /*
                        float num3 = Mathf.Abs(x - rangeLPos.xMax);
                        float num4 = Mathf.Abs(x - (rangeRPos.x - 16f));
                        Widgets.curDragEnd = ((num3 < num4) ? Widgets.RangeEnd.Min : Widgets.RangeEnd.Max);
                        */
                    }
                    flag = true;
                    Event.current.Use();
                    SoundDefOf.DragSlider.PlayOneShotOnCamera(null);
                }
                if (flag || (Event.current.type == EventType.MouseDrag))
                {
                    int num5 = Mathf.RoundToInt(Mathf.Clamp((Event.current.mousePosition.x - rect.x) / rect.width * (float)(max - min) + (float)min, (float)min, (float)max));

                    //Value Selection
                    if (Widgets.curDragEnd == Widgets.RangeEnd.None)
                    {
                        int newSliderVal = Mathf.RoundToInt(Mathf.Clamp((Event.current.mousePosition.x - rect.x) / rect.width * (float)(max - min) + (float)min, (float)min, (float)max));
                        value = Mathf.Clamp(newSliderVal, min, max);
                    }
                    //Range selection
                    if (Widgets.curDragEnd == Widgets.RangeEnd.Min)
                    {
                        if (num5 != range.min)
                        {
                            range.min = num5;
                            if (range.min > max - minWidth)
                            {
                                range.min = max - minWidth;
                            }
                            int num6 = Mathf.Max(min, range.min + minWidth);
                            if (range.max < num6)
                            {
                                range.max = num6;
                            }
                        }
                    }
                    else if (Widgets.curDragEnd == Widgets.RangeEnd.Max && num5 != range.max)
                    {
                        range.max = num5;
                        if (range.max < min + minWidth)
                        {
                            range.max = min + minWidth;
                        }
                        int num7 = Mathf.Min(max, range.max - minWidth);
                        if (range.min > num7)
                        {
                            range.min = num7;
                        }
                    }
                    Widgets.CheckPlayDragSliderSound();
                    Event.current.Use();
                }
            }
        }

        private void ElementListing(Rect leftRect, IKeyFramedElement element)
        {
            TRWidgets.DrawBox(leftRect, SelectedElement  == element ? Color.cyan : TRMats.White05, 1);
            Widgets.Label(leftRect, "element wip");
            if (Widgets.ButtonImage(leftRect.RightPartPixels(20), TiberiumContent.DeleteX))
            {
                this.framedElements[element].Clear();
            }
        }

        private void ElementTimeLine(Rect rightRect, IKeyFramedElement element)
        {
            GUI.color = SelectedElement == element ? Color.white : TRMats.White025;
            var yPos = rightRect.y + rightRect.height / 2f;
            Widgets.DrawLineHorizontal(rightRect.x, yPos, rightRect.width);
            GUI.color = Color.white;

            GetKeyFrames(element, out var frame1, out var frame2, out _);
            var elements = framedElements[element];
            foreach (var keyFrame in elements)
            {
                if (keyFrame.Value.Equals(frame1))
                {
                    GUI.color = Color.magenta;
                }

                if (keyFrame.Value.Equals(frame2))
                {
                    GUI.color = Color.cyan;
                }

                Rect rect = new Vector2(rightRect.x + keyFrame.Value.frameTick * 4, yPos).RectOnPos(new Vector2(16, 16));
                Widgets.DrawTextureFitted(rect, TiberiumContent.KeyFrame, 1f);

                GUI.color = Color.white;
            }
        }

        public static float SCROLLVAL = 1;

        private void TimeControlButtons(Rect playRect, Rect topSettingPart)
        {
            if (Widgets.ButtonImage(playRect, TiberiumContent.PlayPause))
            {
                ticker.TogglePlay();
            }

            GUI.BeginGroup(topSettingPart);
            WidgetRow row = new WidgetRow();
            if (row.ButtonIcon(TiberiumContent.AddKeyFrame))
            {
                foreach (var element in framedElements.Keys)
                {
                    SetKeyFrameFor(element);
                }
            }

            row.Slider(125, ref zoomFactor, zoomRange.min, zoomRange.max);
            row.Slider(125, ref SCROLLVAL, 0.5f, 2);


            row.Label($"{ticker.CurrentTick}");
            row.Init(0, Rect.height - 16, UIDirection.LeftThenUp);
            GUI.EndGroup();
        }
    }
}
