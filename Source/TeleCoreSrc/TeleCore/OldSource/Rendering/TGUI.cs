using System;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace TeleCore.Rendering;

public struct WidgetRow
{
    public float startX;
    public float curX;
    public float curY;
    public float maxWidth = 99999f;
    public float gap;
    public UIDirection growDirection = UIDirection.RightThenUp;

    public WidgetRow(float x, float y, UIDirection growDirection = UIDirection.RightThenUp, float maxWidth = 99999f,
        float gap = 4f)
    {
        this.growDirection = growDirection;
        startX = x;
        curX = x;
        curY = y;
        this.maxWidth = maxWidth;
        this.gap = gap;
    }

    public const float IconSize = 24f;
    public const float DefaultGap = 4f;
    private const float DefaultMaxWidth = 99999f;
    public const float LabelGap = 2f;
    public const float ButtonExtraSpace = 16f;

    public static WidgetRow Empty { get; } = new()
    {
        startX = 0,
        curX = 0,
        curY = 0,
        maxWidth = 0,
        gap = 0,
        growDirection = UIDirection.LeftThenUp
    };
}

public struct RectBounding
{
    public float top;
    public float right;
    public float bottom;
    public float left;

    public RectBounding(params float[] all)
    {
    }
}

public struct RectGridLayout
{
    private Rect gridRect;
    private readonly float columnWidth;
    private readonly float rowHeight;

    public RectGridLayout(Rect rect, int columns = 1, int rows = 1)
    {
        gridRect = rect;
        columnWidth = rect.width / columns;
        rowHeight = rect.height / rows;
        Columns = new Rect[columns];
        Rows = new Rect[rows];

        FillColumnRows(columns, rows);
    }

    public RectGridLayout(Rect rect, float desiredColumnWidth, float desiredRowHeight)
    {
        gridRect = rect;
        columnWidth = desiredColumnWidth;
        rowHeight = desiredRowHeight;
        Columns = new Rect[Mathf.RoundToInt(rect.width / desiredColumnWidth)];
        Rows = new Rect[Mathf.RoundToInt(rect.height / desiredRowHeight)];

        FillColumnRows(Columns.Length, Rows.Length);
    }

    private void FillColumnRows(int columns = 1, int rows = 1)
    {
        //var rectMaker = new RectAggregator(new Rect(gridRect.position, Vector2.zero), this.GetHashCode());

        float columnX = 0;
        for (var i = 0; i < columns; i++)
        {
            Columns[i] = new Rect(columnX, 0, columnWidth, gridRect.height);
            columnX += columnWidth;
        }

        float rowY = 0;
        for (var i = 0; i < rows; i++)
        {
            Rows[i] = new Rect(0, rowY, gridRect.width, rowHeight);
            rowY += rowHeight;
        }
    }

    public Rect Rect => gridRect;

    public Rect[] Columns { get; }

    public Rect[] Rows { get; }
}

public static class TGUI
{
    #region WidgetRowData

    public static class Row
    {
        private static WidgetRow _rowInt;

        public static void BeginRow(float x, float y, UIDirection growDirection = UIDirection.RightThenUp,
            float maxWidth = 99999f, float gap = 4f)
        {
            _rowInt = new WidgetRow(x, y, growDirection, maxWidth, gap);
        }

        public static void EndRow()
        {
            _rowInt = WidgetRow.Empty;
        }

        public static float LeftX(float elementWidth)
        {
            if (_rowInt.growDirection is UIDirection.RightThenUp or UIDirection.RightThenDown) return _rowInt.curX;
            return _rowInt.curX - elementWidth;
        }

        private static void IncrementPosition(float amount)
        {
            if (_rowInt.growDirection == UIDirection.RightThenUp || _rowInt.growDirection == UIDirection.RightThenDown)
                _rowInt.curX += amount;
            else
                _rowInt.curX -= amount;
            if (Mathf.Abs(_rowInt.curX - _rowInt.startX) > _rowInt.maxWidth) IncrementY();
        }

        private static void IncrementY()
        {
            if (_rowInt.growDirection is UIDirection.RightThenUp or UIDirection.LeftThenUp)
                _rowInt.curY -= 24f + _rowInt.gap;
            else
                _rowInt.curY += 24f + _rowInt.gap;
            _rowInt.curX = _rowInt.startX;
        }

        private static void IncrementYIfWillExceedMaxWidth(float width)
        {
            if (Mathf.Abs(_rowInt.curX - _rowInt.startX) + Mathf.Abs(width) > _rowInt.maxWidth) IncrementY();
        }

        #region RowPartMakers

        public static void Gap(float width)
        {
            if (Math.Abs(_rowInt.curX - _rowInt.startX) > 0) IncrementPosition(width);
        }

        public static Rect Label(string text, float width = -1f, string tooltip = null, float height = -1f)
        {
            if (height < 0f) height = 24f;
            if (width < 0f) width = Text.CalcSize(text).x;
            IncrementYIfWillExceedMaxWidth(width + 2f);
            IncrementPosition(2f);
            var rect = new Rect(LeftX(width), _rowInt.curY, width, height);
            Widgets.Label(rect, text);
            if (!tooltip.NullOrEmpty()) TooltipHandler.TipRegion(rect, tooltip);
            IncrementPosition(2f);
            IncrementPosition(rect.width);
            return rect;
        }

        public static bool ButtonIcon(Texture2D tex, string tooltip = null, Color? mouseoverColor = null,
            Color? backgroundColor = null, Color? mouseoverBackgroundColor = null, bool doMouseoverSound = true,
            float overrideSize = -1f)
        {
            var num = overrideSize > 0f ? overrideSize : 24f;
            var num2 = (24f - num) / 2f;
            IncrementYIfWillExceedMaxWidth(num);
            var rect = new Rect(LeftX(num) + num2, _rowInt.curY + num2, num, num);
            if (doMouseoverSound) MouseoverSounds.DoRegion(rect);
            if (mouseoverBackgroundColor != null && Mouse.IsOver(rect))
                Widgets.DrawRectFast(rect, mouseoverBackgroundColor.Value);
            else if (backgroundColor != null && !Mouse.IsOver(rect)) Widgets.DrawRectFast(rect, backgroundColor.Value);
            var result = Widgets.ButtonImage(rect, tex, Color.white, mouseoverColor ?? GenUI.MouseoverColor);
            IncrementPosition(num);
            if (!tooltip.NullOrEmpty()) TooltipHandler.TipRegion(rect, tooltip);
            return result;
        }

        #endregion
    }

    #endregion

    #region UIGrid

    public static void BeginGrid(Rect inRect)
    {
    }

    public static void EndGrid()
    {
    }

    public static void Label(int column = 0, int row = 0)
    {
    }

    #endregion
}

public static class TGUIUtility
{
}