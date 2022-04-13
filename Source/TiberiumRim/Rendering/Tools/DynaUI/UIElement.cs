using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public enum UIElementMode
    {
        Dynamic,
        Static
    }

    public enum UIElementState
    {
        Open,
        Collapsed, 
        Closed
    }

    public abstract class UIElement : IDraggable, IResizable, IFocusable
    {
        //Global Const
        private const int _BorderMargin = 25;

        //
        private UIElementMode uiModeInt = UIElementMode.Dynamic;
        protected UIElement parent;

        //Local Data
        protected Color bgColor = TRMats.MenuSectionBGFillColor;
        protected Color borderColor = TRMats.MenuSectionBGBorderColor;

        protected string label, title;
        protected bool hasTopBar = true;

        private Rect? overrideRect;
        private Vector2 position = Vector2.zero;
        private Vector2 size;

        //
        private Vector2? startDragPos, endDragPos, oldPos;

        public Vector2 StartDragPos => startDragPos ?? Vector2.zero;
        public Vector2 CurrentDragDiff => endDragPos.HasValue ? endDragPos.Value - startDragPos.Value : Vector2.zero;
        protected Vector2 CurrentNewPos => oldPos.HasValue ? new Vector2(oldPos.Value.x + CurrentDragDiff.x, oldPos.Value.y + CurrentDragDiff.y) : position;


        public UIElementMode UIMode
        {
            get => uiModeInt;
            private set => uiModeInt = value;
        }

        public virtual int Priority => 999;

        public int RenderLayer { get; set; } = 0;

        public bool CanAcceptAnything => IsActive && UIState == UIElementState.Open;

        public UIElementState UIState { get; protected set; } = UIElementState.Open;

        protected bool IsFocused => UIEventHandler.IsFocused(this);
        protected bool IsLocked { get; private set; }
        protected bool ClickedIntoTop { get; private set; }


        public virtual bool CanBeFocused => true;

        public bool IsActive { get; set; } = true;

        public object DragAndDropData { get; protected set; }

        public Vector2 Position
        {
            get => position;
            set
            {
                position = value;
                overrideRect = new Rect(position.x, position.y, size.x, size.y);
                Notify_StateChanged();
            }
        }
        public Vector2 Size
        {
            get => size;
            set
            {
                size = value;
                overrideRect = new Rect(position.x, position.y, size.x, size.y);
                Notify_StateChanged();
            }
        }

        public Rect Rect
        {
            get => (overrideRect ?? new Rect(position, size)).Rounded();
            private set
            {
                overrideRect = value;
                position = new Vector2(value.x, value.y);
                size = new Vector2(value.width, value.height);
                Notify_StateChanged();
            }
        }

        public Rect InRect => new Rect(Rect.x, hasTopBar? TopRect.yMax : Rect.y, Rect.width, Rect.height - TopRect.height);

        public Rect? DragContext => parent?.Rect ?? null;

        public string Title
        {
            get => title;
            protected set => title = value;
        }

        public virtual string Label => "New Element";

        //Input Rect Data
        protected Rect TopRect => new Rect(position.x, position.y, size.x, _BorderMargin);
        protected virtual Rect DragAreaRect => TopRect;
        public virtual Rect FocusRect => Rect;

        protected bool CanMove => UIMode == UIElementMode.Dynamic && ClickedIntoTop && !IsLocked && IsInDragArea();

        //Constructors
        protected UIElement(UIElementMode mode)
        {
            this.UIMode = mode;
        }

        protected UIElement(Rect rect, UIElementMode mode)
        {
            this.Rect = rect;
            this.UIMode = mode;
        }

        protected UIElement(Vector2 pos, Vector2 size, UIElementMode mode)
        {
            this.size = size;
            Position = pos;
            this.UIMode = mode;
        }

        //Public Data Transfer
        public void ToggleOpen()
        {
            if (parent is not ToolBar) return;
            UIState = UIState == UIElementState.Open ? UIElementState.Closed : UIElementState.Open;
        }

        public void SetPosition(Vector2 newPos)
        {
            Position = newPos;
        }

        public void SetSize(Vector2 newSize)
        {
            Size = newSize;
        }

        public void SetData(Vector2? pos = null, Vector2? size = null, UIElement parent = null)
        {
            this.parent = parent;
            this.Size = size ?? this.size;
            this.Position = pos ?? position;
        }

        //Inner Data Handling
        private void Notify_StateChanged()
        {
            parent?.Notify_ChildElementChanged(this);
        }

        protected virtual void Notify_ChildElementChanged(UIElement element)
        {

        }

        //
        protected virtual bool IsInDragArea()
        {
            return Mouse.IsOver(DragAreaRect);
        }


        //Drawing
        public void DrawElement(Rect? overrideRect = null)
        {
            //UIEventHandler.CurrentLayer++;
            //RenderLayer = UIEventHandler.CurrentLayer;
            UIEventHandler.RegisterLayer(this);
            if (UIState == UIElementState.Closed) return;

            if (overrideRect != null)
                this.Rect = overrideRect.Value;

            //
            HandleEvent();

            //
            TRWidgets.DrawColoredBox(Rect, bgColor, borderColor, 1);
            if(hasTopBar) DoTopBar();

            //
            if (UIState == UIElementState.Collapsed) return;

            //Custom Drawing
            DragAndDropData = null;
            DrawContents(InRect.ContractedBy(1));
        }

        private void DoTopBar()
        {
            //
            TRWidgets.DrawBoxHighlightIfMouseOver(DragAreaRect);
            if (title != null)
            {
                Text.Font = GameFont.Small;
                Text.Anchor = TextAnchor.UpperLeft;
                Widgets.Label(TopRect, Title);
            }
            Text.Anchor = default;
            Text.Font = default;

            //Draw Element Settings
            DoSettings(TopRect);
        }

        private void DoSettings(Rect rect)
        {
            Widgets.BeginGroup(rect);
            WidgetRow row = new WidgetRow();
            if (UIMode == UIElementMode.Dynamic)
            {
                if (row.ButtonIcon(IsLocked ? TiberiumContent.LockClosed : TiberiumContent.LockOpen))
                {
                    IsLocked = !IsLocked;
                }
            }

            if (parent is ToolBar)
            {
                if (row.ButtonIcon(TiberiumContent.DeleteX))
                {
                    ToggleOpen();
                }
            }

            row.Init(rect.width - (WidgetRow.IconSize * 2 + 1), rect.height - (WidgetRow.IconSize + 1), UIDirection.RightThenDown);
            Widgets.EndGroup();
        }

        //Event Handling
        private void HandleEvent()
        {
            Event curEvent = Event.current;
            Vector2 mousePos = curEvent.mousePosition;

            bool isInContext = Rect.Contains(mousePos);
            //if (!isInContext) return;

            if (curEvent.type == EventType.MouseDown && isInContext)
            {
                startDragPos ??= mousePos;
                oldPos ??= position;
                UIEventHandler.StartFocus(this);

                if (Mouse.IsOver(TopRect))
                {
                    ClickedIntoTop = true;
                }

                //FloatMenu
                if (curEvent.button == 1 && Mouse.IsOver(FocusRect))
                {
                    var options = RightClickOptions()?.ToList();
                    if (options != null && options.Any())
                    {
                        FloatMenu menu = new FloatMenu(options);
                        menu.vanishIfMouseDistant = true;
                        Find.WindowStack.Add(menu);
                    }
                }
            }

            if (curEvent.type == EventType.MouseDrag && startDragPos != null)
            {
                endDragPos = mousePos;
            }

            //Custom Handling
            HandleEvent_Custom(curEvent, isInContext);

            //Handle Pos Manipulation
            if (IsFocused)
            {
                if (UIDragger.IsBeingDragged(this) || (curEvent.type == EventType.MouseDrag && CanMove))
                    UIDragger.Notify_ActiveDrag(this, curEvent);

                if (UIDragNDropper.IsSource(this) || DragAndDropData != null && curEvent.type == EventType.MouseDrag)
                    UIDragNDropper.Notify_DraggingData(this, DragAndDropData, curEvent);
            }

            //Reset
            if (curEvent.type == EventType.MouseUp)
            {
                startDragPos = oldPos = endDragPos = null;
                ClickedIntoTop = false;
                UIEventHandler.StopFocus(this);
            }
        }

        protected virtual IEnumerable<FloatMenuOption> RightClickOptions()
        {
            return null;
        }

        //
        protected virtual void HandleEvent_Custom(Event ev, bool inContext = false)
        {
        }

        protected virtual void DrawContents(Rect inRect)
        {
        }
    }
}
