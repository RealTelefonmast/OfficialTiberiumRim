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

    public struct KeyFrameData : IExposable
    {
        // Size(0,1) | Pos(2,3) Rot(4)
        [Unsaved]
        private string[] stringBuffers;

        //TRS
        private float rotation;
        private Vector2 position, size;
        public float layerPos;

        public string[] ValueBuffer => stringBuffers;

        public Vector2 TSize
        {
            get => size;
            set => size = value;
        }

        public Vector2 TPosition
        {
            get => position;
            set => position = value;
        }

        public float TRotation
        {
            get => rotation;
            set => rotation = value;
        }

        public void UpdateBuffer()
        {
            var tSize = TSize;
            var tPos = TPosition;
            var tRot = TRotation;
            stringBuffers[0] = tSize.x.ToString();
            stringBuffers[1] = tSize.y.ToString();
            stringBuffers[2] = tPos.x.ToString();
            stringBuffers[3] = tPos.y.ToString();
            stringBuffers[4] = tRot .ToString();
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref rotation, "rotation", forceSave: true);
            Scribe_Values.Look(ref position, "position", forceSave: true);
            Scribe_Values.Look(ref size, "size", forceSave: true);
            Scribe_Values.Look(ref layerPos, "layerPos", forceSave: true);
        }

        public KeyFrameData(Vector2 pos, float rot, Vector2 size)
        {
            position = pos;
            rotation = rot;
            this.size = size;
            stringBuffers = new string[5]
            {
                size.x.ToString(),
                size.y.ToString(),
                pos.x.ToString(),
                pos.y.ToString(),
                rot.ToString()
            };
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

    public struct KeyFrame : IExposable
    {
        private KeyFrameData frameData;
        private int frameTick;

        public KeyFrameData Data => frameData;
        public int Frame => frameTick;

        public void ExposeData()
        {
            Scribe_Values.Look(ref frameTick, "frameTick");
            Scribe_Deep.Look(ref frameData, "frameData");
        }

        public KeyFrame(KeyFrameData data, int tick)
        {
            frameData = data;
            frameTick = tick;
        }

        public float Second => frameTick.TicksToSeconds();
    }

    public class TimeLineControl : UIElement
    {
        //
        private const int _PixelsPerTick = 4;

        private int tickLength;
        private int currentFrameInt;
        private bool isPaused = true;
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
            dist = Mathf.InverseLerp(frame1?.Frame ?? 0, frame2?.Frame ?? 0, CurrentFrame);
            return frame1 != null && frame2 != null;
        }

        public KeyFrameData GetDataFor(IKeyFramedElement element)
        {
            if (!framedElements.ContainsKey(element)) return element.KeyFrameData;
            if (IsAtKeyFrame(element)) return framedElements[element][CurrentFrame].Data;
            if (GetKeyFrames(element, out var frame1, out var frame2, out var lerpVal))
                return frame1.Value.Data.Interpolated(frame2.Value.Data, lerpVal);

            if (frame1.HasValue)
                return frame1.Value.Data;
            if (frame2.HasValue)
                return frame2.Value.Data;

            return element.KeyFrameData;
        }

        public TimeLineControl() : base(UIElementMode.Static)
        {
            TRFind.TickManager.RegisterTickAction(delegate
            {
                if (isPaused) return;
                if (CurrentFrame >= PlayRange.max)
                {
                    CurrentFrame = PlayRange.min;
                    return;
                }
                CurrentFrame++;
            });

            //
            //hasTopBar = false;
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
            //if (data.HasValue)
            {
                //element.SetTRS(data);
                if (framedElements[element].ContainsKey(CurrentFrame))
                {
                    framedElements[element][CurrentFrame] = new KeyFrame(data, CurrentFrame);
                    return;
                }
                framedElements[element].Add(CurrentFrame, new KeyFrame(data, CurrentFrame));
            }
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
            int topSize = 30;
            int leftSize = 125;
            int elementSize = 25;
            int timeLineContract = 10;

            Rect leftRect = inRect.LeftPartPixels(leftSize);
            Rect rightRect = inRect.RightPartPixels(inRect.width - leftSize);

            Rect topLeft = leftRect.TopPartPixels(topSize);
            Rect botLeft = leftRect.BottomPartPixels(inRect.height - topSize);

            Rect topRight = rightRect.TopPartPixels(topSize);
            Rect botRight = rightRect.BottomPartPixels(inRect.height - topSize);

            TimeControlButtons(TopRect.RightPartPixels(TopRect.width - leftSize));

            //Element List Scroller
            int elementListCount = 10; //framedElements.Count
            Rect elementListViewRect = new Rect(botLeft.x, botLeft.y, botLeft.width,
                (elementListCount * elementSize) - botLeft.height);

            //Time Line Scroller
            Rect timelineViewRect =
                new Rect(topRight.x, topRight.y, TimeLineLength, rightRect.height).ExpandedBy(timeLineContract, 0);
            Rect timelineBotViewRect = new Rect(botRight.x, botRight.y, TimeLineLength, elementListViewRect.height);

            //
            float curY = 0;
            Widgets.BeginScrollView(botLeft, ref elementScrollPos, elementListViewRect, false);
            {
                curY = elementListViewRect.y;
                foreach (var element in framedElements.Keys)
                {
                    Rect left = new Rect(botLeft.x, curY, botLeft.width, elementSize);
                    ElementListing(left, element);
                    curY += elementSize;
                }
            }
            Widgets.EndScrollView();

            //
            Widgets.DrawBoxSolid(rightRect, TRMats.BGDarker);
            Widgets.ScrollHorizontal(rightRect, ref timeLineScrollPos, timelineViewRect);
            Widgets.BeginScrollView(rightRect, ref timeLineScrollPos, timelineViewRect, false);
            {
                DrawTimeSelector(timelineViewRect.ContractedBy(timeLineContract, 0));
            }
            Widgets.EndScrollView();

            timeLineScrollPos = new Vector2(timeLineScrollPos.x, elementScrollPos.y);
            GUI.BeginScrollView(botRight, timeLineScrollPos, timelineBotViewRect, GUIStyle.none, GUIStyle.none);
            {
                curY = timelineBotViewRect.y;
                foreach (var element in framedElements.Keys)
                {
                    Rect right =
                        new Rect(timelineBotViewRect.x, curY, TimeLineLength, elementSize).ContractedBy(
                            timeLineContract, 0);
                    ElementTimeLine(right, element);
                    curY += elementSize;
                }
            }
            GUI.EndScrollView();

            TRWidgets.DrawBox(rightRect, TRMats.MenuSectionBGBorderColor, 1);
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

            //Draw Tick Lines
            float curX = timeBar.x;
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
            Rect rangeLBar = new Rect(rangeLPos.position, new Vector2(rangeLPos.width, rect.height));
            Rect rangeRBar = new Rect(rangeRPos.position, new Vector2(rangeRPos.width, rect.height));
            Rect valPos = new Rect(valX-12, rect.y, 25, 25);
            GUI.DrawTexture(rangeLPos,  TiberiumContent.TimeSelRangeL);
            GUI.DrawTexture(rangeRPos, TiberiumContent.TimeSelRangeR);

            Widgets.DrawLineVertical(valX, valPos.yMax, rect.height);
            GUI.DrawTexture(valPos, TiberiumContent.TimeSelMarker);

            valPos = new Rect(valPos.x + 8, valPos.y, valPos.width, valPos.height);
            Widgets.Label(valPos, $"{CurrentFrame}");

            Widgets.DrawHighlightIfMouseover(rangeLBar);
            Widgets.DrawHighlightIfMouseover(rangeRBar);

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
            TRWidgets.DrawBox(leftRect, SelectedElement  == element ? Color.cyan : TRColor.White05, 1);
            Widgets.Label(leftRect, "element wip");
            if (Widgets.ButtonImage(leftRect.RightPartPixels(20), TiberiumContent.DeleteX))
            {
                this.framedElements[element].Clear();
            }
        }

        private void ElementTimeLine(Rect rightRect, IKeyFramedElement element)
        {
            Widgets.DrawHighlightIfMouseover(rightRect);
            GUI.color = SelectedElement == element ? Color.white : TRColor.White025;
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

                Rect rect = new Vector2(rightRect.x + keyFrame.Value.Frame * PixelPerTickAdjusted, yPos).RectOnPos(new Vector2(16, 16));
                Widgets.DrawTextureFitted(rect, TiberiumContent.KeyFrame, 1f);

                GUI.color = Color.white;
            }
        }

        public static float SCROLLVAL = 1;

        public static float ROTATION = 0;

        private void TimeControlButtons(Rect topPart)
        {
            Widgets.BeginGroup(topPart);
            WidgetRow row = new WidgetRow();
            if (row.ButtonIcon(TiberiumContent.PlayPause))
            {
                isPaused = !isPaused;
            }
            if (row.ButtonIcon(TiberiumContent.AddKeyFrame))
            {
                foreach (var element in framedElements.Keys)
                {
                    SetKeyFrameFor(element);
                }
            }

            row.Slider(125, ref zoomFactor, zoomRange.min, zoomRange.max);
            row.Slider(125, ref SCROLLVAL, 0.5f, 2);
            row.Slider(125, ref ROTATION, 0, 360);

            if (Canvas.ActiveTexture != null)
            {
                //Canvas.ActiveTexture.SetTRSP_Direct(rot: Canvas.ActiveTexture.TRotation);
            }

            row.Label($"{TRFind.TickManager.CurrentTick}");
            row.Init(0, Rect.height - 16, UIDirection.LeftThenUp);
            Widgets.EndGroup();
        }
    }
}
