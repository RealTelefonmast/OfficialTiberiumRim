using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace TiberiumRim
{
    public static class TRWidgets
    {
        //
        public static Vector2 Size(this Texture texture)
        {
            return new Vector2(texture.width, texture.height);
        }

        //
        public static Rect RectOnPos(this Vector2 pos, Vector2 size)
        {
            return new Rect(pos.x - size.x / 2f, pos.y - size.y / 2f, size.x, size.y);
        }

        public static Rect RectOnPos(this Vector2 pos, Rect rect)
        {
            return RectOnPos(pos, rect.size);
        }

        public static void Slider(this WidgetRow row, float width, ref float value, float min = 0, float max = 1)
        {
            row.IncrementYIfWillExceedMaxWidth(width);
            Rect rect = new Rect(row.LeftX(width), row.curY, width, 24f);
            value = Widgets.HorizontalSlider(rect, value, min, max, true);
            row.IncrementPosition(width);
        }

        public static Rect TextFieldNumericFix<T>(this WidgetRow row, ref T val, ref string buffer, float width = -1f) where T : struct
        {
            if (width < 0f)
            {
                width = Text.CalcSize(val.ToString()).x;
            }
            row.IncrementYIfWillExceedMaxWidth(width + 2f);
            row.IncrementPosition(2f);
            Rect rect = new Rect(row.LeftX(width), row.curY, width, 24f);
            Widgets.TextFieldNumeric<T>(rect, ref val, ref buffer, float.MinValue, float.MaxValue);
            row.IncrementPosition(2f);
            row.IncrementPosition(rect.width);
            return rect;
        }

        public static void DoBGForNext(this Listing_Standard listing, Color color)
        {
            Rect rect = listing.GetNextRect();
            Widgets.DrawBoxSolid(rect, color);
        }

        public static Rect GetNextRect(this Listing_Standard listing)
        {
            return new Rect(listing.curX, listing.curY, listing.ColumnWidth, Text.LineHeight);
        }

        public static void TextureElement(this Listing_Standard listing, TextureElement tex)
        {
            Rect rect = listing.GetRect(Text.LineHeight);
            if (listing.BoundingRectCached == null || rect.Overlaps(listing.BoundingRectCached.Value))
            {
                DrawTextureWithMaterial(rect, tex.Texture, tex.Material, tex.TexCoords);
            }
        }
        public static void TextFieldNumericLabeled<T>(this Listing_Standard listing, string label, ref T val, ref string buffer, float min = 0f, float max = 1E+09f, TextAnchor anchor = TextAnchor.MiddleRight) where T : struct
        {
            Rect rect = listing.GetRect(Text.LineHeight);
            if (listing.BoundingRectCached == null || rect.Overlaps(listing.BoundingRectCached.Value))
            {
                Rect rect2 = rect.LeftHalf().Rounded();
                Rect rect3 = rect.RightHalf().Rounded();
                TextAnchor oldAnchor = Text.Anchor;
                Text.Anchor = anchor;
                Widgets.Label(rect2, label);
                Text.Anchor = oldAnchor;
                Widgets.TextFieldNumeric(rect3, ref val, ref buffer, min, max);
                //TextFieldNumericFix(rect3, ref val, ref buffer, min, max);
            }
            listing.Gap(listing.verticalSpacing);
        }

        public static void ScrollVertical(Rect outRect, ref Vector2 scrollPosition, Rect viewRect, float ScrollWheelSpeed = 20f)
        {
            if (Event.current.type == EventType.ScrollWheel && Mouse.IsOver(outRect))
            {
                scrollPosition.y += Event.current.delta.y * ScrollWheelSpeed;
                float num = 0f;
                float num2 = viewRect.height - outRect.height + 16f;
                if (scrollPosition.y < num)
                {
                    scrollPosition.y = num;
                }
                if (scrollPosition.y > num2)
                {
                    scrollPosition.y = num2;
                }
                Event.current.Use();
            }
        }

        public static void DrawMaterial(Rect rect, Vector2 pivot, float angle, Material material, Rect texCoords = default(Rect))
        {
            if (Event.current.type != EventType.Repaint) return;

            Matrix4x4 matrix = Matrix4x4.identity;
            if (angle != 0f)
            {
                matrix = GUI.matrix;
                UI.RotateAroundPivot(angle, pivot);
            }

            DrawTextureWithMaterial(rect, material.mainTexture, material, texCoords);

            //GenUI.DrawTextureWithMaterial(rect, material.mainTexture, material, texCoords);

            if (angle != 0f)
            {
                GUI.matrix = matrix;
            }
        }

        public static void DrawTextureFromMat(Rect rect, Vector2 pivot, float angle, Material material, Rect texCoords = default(Rect))
        {
            Matrix4x4 matrix = Matrix4x4.identity;
            if (angle != 0f)
            {
                matrix = GUI.matrix;
                UI.RotateAroundPivot(angle, pivot);
            }

            GenUI.DrawTextureWithMaterial(rect, material.mainTexture, null, texCoords);
            //GUI.DrawTextureWithTexCoords(rect, material.mainTexture, texCoords);

            if (angle != 0f)
            {
                GUI.matrix = matrix;
            }
        }

        public static void DrawTextureWithMaterial(Rect rect, Texture texture, Material material, Rect texCoords = default(Rect))
        {
            if (texCoords == default(Rect))
            {
                if (material == null)
                {
                    GUI.DrawTexture(rect, texture);
                    return;
                }
                if (Event.current.type == EventType.Repaint)
                {
                    Graphics.DrawTexture(rect, texture, new Rect(0f, 0f, 1f, 1f), 0, 0, 0, 0, new Color(GUI.color.r * 0.5f, GUI.color.g * 0.5f, GUI.color.b * 0.5f, GUI.color.a * 0.5f), material);
                    return;
                }
            }
            else
            {
                if (material == null)
                {
                    GUI.DrawTextureWithTexCoords(rect, texture, texCoords);
                    return;
                }
                if (Event.current.type == EventType.Repaint)
                {
                    Graphics.DrawTexture(rect, texture, texCoords, 0, 0, 0, 0, new Color(GUI.color.r * 0.5f, GUI.color.g * 0.5f, GUI.color.b * 0.5f, GUI.color.a * 0.5f), material);
                }
            }
        }

        public static void DrawBarMarkerAt(Rect barRect, float pct)
        {
            float num = barRect.height;
            Vector2 vector = new Vector2(barRect.x + barRect.width * pct, barRect.y);
            Rect rect = new Rect(vector.x - num / 2f, vector.y, num, num);
            var matrix = GUI.matrix;
            UI.RotateAroundPivot(180f, rect.center);
            GUI.DrawTexture(rect, Need.BarInstantMarkerTex);
            GUI.matrix = matrix;
        }

        public static void SliderCustomVertical(Rect rect, ref float value)
        {
            var previousVal = value;
            value = (float)Math.Round(GUI.VerticalSlider(rect, value, 0f, 1f), 2);
            if (Math.Abs(previousVal - value) > 0.01f)
            {
                SoundDefOf.DragSlider.PlayOneShotOnCamera(null);
            }
        }

        public static float HorizontalSlider(Rect rect, float value, float min, float max, float roundTo = -1f)
        {
            float num = GUI.HorizontalSlider(rect, value, min, max);
            if (roundTo > 0f)
            {
                num = (float)Mathf.RoundToInt(num / roundTo) * roundTo;
            }
            if (value != num)
            {
                SoundDefOf.DragSlider.PlayOneShotOnCamera(null);
            }
            return num;
        }

        public static float VerticalSlider(Rect rect, float value, float min, float max, float roundTo = -1f)
        {
            float num = GUI.VerticalSlider(rect, value, max, min);
            if (roundTo > 0f)
            {
                num = (float)Mathf.RoundToInt(num / roundTo) * roundTo;
            }
            if (value != num)
            {
                SoundDefOf.DragSlider.PlayOneShotOnCamera(null);
            }
            return num;
        }

        public static void SliderCustom(Rect rect, int id, ref int value, int min = 0, int max = 100, Texture sliderTexture = null, Color sliderColor = default)
        {
            Rect rect2 = rect;
            rect2.xMin += 8f;
            rect2.xMax -= 8f;

            Rect position = new Rect(rect2.x, rect2.yMax - 8f - 1f, rect2.width, 2f);
            GUI.DrawTexture(position, BaseContent.WhiteTex);
            GUI.color = Color.white;
            float num = rect2.x + rect2.width * (float)(value - min) / (float)(max - min);
            Rect position2 = new Rect(num - 16f, position.center.y - 8f, 16f, 16f);
            GUI.DrawTexture(position2, sliderTexture ?? TiberiumContent.TimeSelMarker);

            if (Event.current.type == EventType.MouseUp || Event.current.rawType == EventType.MouseDown)
            {
                Widgets.draggingId = 0;
                SoundDefOf.DragSlider.PlayOneShotOnCamera(null);
            }

            bool flag = false;
            if (Mouse.IsOver(rect) || Widgets.draggingId == id)
            {
                if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && id != Widgets.draggingId)
                {
                    Widgets.draggingId = id;
                    /*
                    float x = Event.current.mousePosition.x;
                    if (x < position2.xMax)
                    {
                        Widgets.curDragEnd = Widgets.RangeEnd.Min;
                    }
                    else if (x > position3.xMin)
                    {
                        Widgets.curDragEnd = Widgets.RangeEnd.Max;
                    }
                    else
                    {
                        float num3 = Mathf.Abs(x - position2.xMax);
                        float num4 = Mathf.Abs(x - (position3.x - 16f));
                        Widgets.curDragEnd = ((num3 < num4) ? Widgets.RangeEnd.Min : Widgets.RangeEnd.Max);
                    }
                    */
                    flag = true;
                    Event.current.Use();
                    SoundDefOf.DragSlider.PlayOneShotOnCamera(null);
                }
                if (flag || Event.current.type == EventType.MouseDrag)
                {
                    int newSliderVal = Mathf.RoundToInt(Mathf.Clamp((Event.current.mousePosition.x - rect2.x) / rect2.width * (float)(max - min) + (float)min, (float)min, (float)max));
                    value = Mathf.Clamp(newSliderVal, min, max);
                    Widgets.CheckPlayDragSliderSound();
                    Event.current.Use();
                }
            }

        }

        public static void DrawGridOnCenter(Rect rect, float gridSize, Vector2 center)
        {
            Widgets.BeginGroup(rect);
            rect = rect.AtZero();

            float xSize, ySize = xSize = 0;
            int xCount, yCount = xCount = 0;

            xCount = Mathf.CeilToInt(rect.width / gridSize);
            yCount = Mathf.CeilToInt(rect.height / gridSize);
            xSize = ySize = gridSize;

            Widgets.DrawBoxSolid(rect.center.RectOnPos(new Vector2(4,4)), Color.magenta);
            Widgets.DrawBoxSolid(center.RectOnPos(new Vector2(4, 4)), Color.green);

            for (int x = 0; x < xCount; x++)
            {
                var curY = center.y + (x * xSize) - ((0.5f * rect.height));
                var startX = new Vector2(0, curY);
                var endX = new Vector2(rect.width, curY);

                Widgets.DrawLine(startX, endX, Color.red, 1);
            }

            for (int y = 0; y < yCount; y++)
            {
                var curX = center.x + (y * ySize) - (0.5f * rect.width);
                var startY = new Vector2(curX, 0);
                var endY = new Vector2(curX, rect.height);
                Widgets.DrawLine(startY, endY, Color.red, 1);
            }

            Widgets.EndGroup();
        }

        public static void DrawGrid(Rect inRect, float value, float scale = 1, Vector2 origin = default, bool asCellAmount = false)
        {
            Widgets.BeginGroup(inRect);
            float xSize, ySize = xSize = 0;
            float xCount, yCount = xCount = 0;

            value *= scale;
            if (asCellAmount)
            {
                 xSize = (inRect.width / value);
                 ySize = (inRect.height / value);
                 xCount = yCount = value;
            }
            else
            {
                xCount = (inRect.width / value);
                yCount = (inRect.height / value);
                xSize = ySize = value;
            }

            var xH = (int)(xCount / 2f);
            var yH = (int)(yCount / 2f);
            for (int x = -xH; x <= xH; x++)
            {
                for (int y = -yH; y <= yH; y++)
                {
                    var xVal = (x * xSize + (0.5f * (xCount * xSize))) + origin.x;
                    var yVal = (y * ySize + (0.5f * (yCount * ySize))) + origin.y;
                    var xPos = new Vector2(xVal, 0);
                    var xPosEnd = new Vector2(xVal, inRect.height);
                    var yPos = new Vector2(0, yVal);
                    var yPosEnd = new Vector2(inRect.width, yVal);
                    Widgets.DrawLine(xPos, xPosEnd, TRColor.White005, 1);
                    Widgets.DrawLine(yPos, yPosEnd, TRColor.White005, 1);
                }
            }
            Widgets.EndGroup();
        }

        public static void DrawTextureInCorner(Rect rect, Texture2D texture, float textureWidth, TextAnchor anchor, Vector2 offset = default)
        {
            Rect newRect = new Rect();
            Vector2 size = FittedSizeFor(texture, textureWidth);
            switch (anchor)
            {
                case TextAnchor.UpperLeft:
                    newRect = new Rect(rect.x, rect.y, size.x, size.y);
                    break;
                case TextAnchor.LowerLeft:
                    newRect = new Rect(rect.x, rect.yMax - size.y, size.x, size.y);
                    break;
                case TextAnchor.UpperRight:
                    newRect = new Rect(rect.xMax - size.x, rect.y, size.x, size.y);
                    break;
                case TextAnchor.LowerRight:
                    newRect = new Rect(rect.xMax - size.x, rect.yMax - size.y, size.x, size.y);
                    break;
            }
            newRect.Set(newRect.x + offset.x, newRect.y + offset.y, newRect.width, newRect.height);
            Widgets.DrawTextureFitted(newRect, texture, 1f);
        }

        public static void AddGapLine(ref float curY, float width, float gapSize, float x = 0, TextAnchor anchor = TextAnchor.MiddleCenter)
        {
            //Adds 
            GUI.color = TRColor.GapLineColor;
            var yPos = curY;
            switch (anchor)
            {
                case TextAnchor.MiddleCenter:
                    yPos = curY + (gapSize / 2f);
                    break;
                case TextAnchor.UpperCenter:
                    yPos = curY;
                    break;
                case TextAnchor.LowerCenter:
                    yPos = curY + gapSize;
                    break;
            }
            Widgets.DrawLineHorizontal(x, yPos, width);
            curY += gapSize;
            GUI.color = Color.white;
        }

        public static void DrawTexture(float x, float y, float width, Texture2D texture, out float height)
        {
            Vector2 dimensions = new Vector2(texture.width, texture.height);
            float mainPct = dimensions.x / width;
            dimensions /= mainPct;
            height = dimensions.y;
            Rect rect = new Rect(x, y, dimensions.x, dimensions.y);
            Widgets.DrawTextureFitted(rect, texture, 1f);
        }

        public static Vector2 FittedSizeFor(Texture2D texture, float width)
        {
            Vector2 dimensions = new Vector2(texture.width, texture.height);
            float mainPct = dimensions.x / width;
            dimensions /= mainPct;
            return dimensions;
        }

        public static bool ButtonColoredHighlight(Rect rect, string label, Color bgColor, bool mouseOverSound = true, int thickness = 1)
        {
            Color borderColor = new Color(
                Mathf.Clamp(2 * bgColor.r, 0f, 1f),
                Mathf.Clamp(2 * bgColor.g, 0f, 1f),
                Mathf.Clamp(2 * bgColor.b, 0f, 1f));
            return ButtonColoredHighlight(rect, label, bgColor, borderColor, mouseOverSound, thickness);
        }

        public static bool ButtonColoredHighlight(Rect rect, string label, Color bgColor, Color borderColor, bool mouseOverSound = true, int thickness = 1)
        {
            DrawColoredBox(rect, bgColor, borderColor, thickness);

            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(rect, label);
            Text.Anchor = default;

            Widgets.DrawHighlightIfMouseover(rect);
            return Widgets.ButtonInvisible(rect, mouseOverSound);
        }

        public static void DrawBoxHighlightIfMouseOver(Rect rect)
        {
            if(Mouse.IsOver(rect))
                DrawBoxHighlight(rect);
        }

        public static void DrawBoxHighlight(Rect rect)
        {
            DrawBox(rect, TRColor.White025, 1);
        }

        public static void DrawBoxIfMouseOver(Rect rect, Color color)
        {
            if (Mouse.IsOver(rect))
                DrawBox(rect, color, 1);
        }

        public static void DrawBox(Rect rect, float opacity, int thickness)
        {
            DrawBox(rect, new Color(1, 1, 1, opacity), thickness);
        }

        public static void DrawBox(Rect rect, Color color, int thickness)
        {
            Color oldColor = GUI.color;
            GUI.color = color;
            Widgets.DrawBox(rect, thickness);
            GUI.color = oldColor;
        }

        public static void DrawColoredBox(Rect rect, Color fillColor, Color borderColor, int thickness)
        {
            Color oldColor = GUI.color;
            Widgets.DrawBoxSolid(rect, fillColor);
            GUI.color = borderColor;
            Widgets.DrawBox(rect, thickness);
            GUI.color = oldColor;
        }

        public static Color ColorFor(Enum enumType)
        {
            if (enumType is TiberiumValueType tibType)
            {
                return tibType.GetColor();
            }

            if (enumType is AtmosphericValueType atmosType)
            {

            }
            return Color.white;
        }

        public static void DoTinyLabel(Rect rect, string label)
        {
            Text.Font = GameFont.Tiny;
            Text.Anchor = TextAnchor.UpperLeft;
            Widgets.Label(rect, label);

            Text.Anchor = default;
            Text.Font = default;
        }

        public static float DrawNetworkValueTypeReadout(Rect rect, GameFont font, float textYOffset, NetworkContainerSet containerSet)
        {
            float height = 5;

            Widgets.BeginGroup(rect);
            Text.Font = font;
            Text.Anchor = TextAnchor.UpperLeft;
            foreach (var type in containerSet.AllTypes)
            {
                // float value = GetNetwork(Find.CurrentMap).NetworkValueFor(type);
                //if(value <= 0) continue;
                string label = $"{type}: {containerSet.GetValueByType(type)}";
                Rect typeRect = new Rect(5, height, 10, 10);
                Vector2 typeSize = Text.CalcSize(label);
                Rect typeLabelRect = new Rect(20, height + textYOffset, typeSize.x, typeSize.y);
                Widgets.DrawBoxSolid(typeRect, type.valueColor);
                Widgets.Label(typeLabelRect, label);
                height += 10 + 2;
            }
            Text.Font = default;
            Text.Anchor = default;
            Widgets.EndGroup();

            return height;
        }

        public static bool MouseClickIn(Rect rect, int mouseButton)
        {
            Event curEvent = Event.current;
            return Mouse.IsOver(rect) && curEvent.type == EventType.MouseDown && curEvent.button == mouseButton;
        }

        public static void DrawListedPart<T>(Rect rect, ref Vector2 scrollPos, List<T> elements, Action<Rect, UIPartSizes, T> drawProccessor, Func<T, UIPartSizes> heightFunc)
        {
            var height = elements.Sum(s => heightFunc(s).totalSize);
            var viewRect = new Rect(rect.x, rect.y, rect.width, height);
            Widgets.BeginScrollView(rect, ref scrollPos, viewRect, false);
            float curY = rect.y;
            for (var i = 0; i < elements.Count; i++)
            {
                var element = elements[i];
                var listingHeight = heightFunc(element);
                var listingRect = new Rect(rect.x, curY, rect.width, listingHeight.totalSize);
                if (i % 2 == 0)
                {
                    Widgets.DrawHighlight(listingRect);
                }
                drawProccessor(listingRect, listingHeight, element);
                curY += listingHeight.totalSize;
            }
            Widgets.EndScrollView();
        }

        public static void AbsorbInput(Rect rect)
        {
            var curEv = Event.current;
            if (Mouse.IsOver(rect) && curEv.type == EventType.MouseDown)
                curEv.Use();
        }

        public static void DrawHighlightColor(Rect rect, Color color)
        {
            var oldColor = GUI.color;
            GUI.color = color;
            Widgets.DrawHighlight(rect);
            GUI.color = oldColor;
        }

        public static void DrawHighlightIfMouseOverColor(Rect rect, Color color)
        {
            var oldColor = GUI.color;
            GUI.color = color;
            Widgets.DrawHighlightIfMouseover(rect);
            GUI.color = oldColor;
        }

        public static Vector2 GetTiberiumReadoutSize(NetworkContainer container)
        {
            Vector2 size = new Vector2(10, 10);
            foreach (var type in container.AllStoredTypes)
            {
                Vector2 typeSize = Text.CalcSize($"{type.labelShort}: {container.ValueForType(type)}");
                size.y += 10 + 2;
                var sizeX = typeSize.x + 20;
                if (size.x <= sizeX)
                    size.x += sizeX;
            }
            return size;
        }

        public static void DrawTiberiumReadout(Rect rect, NetworkContainer container)
        {
            float height = 5;
            Widgets.BeginGroup(rect);
            Text.Font = GameFont.Tiny;
            Text.Anchor = TextAnchor.UpperLeft;
            foreach (var type in container.AllStoredTypes)
            {
                // float value = GetNetwork(Find.CurrentMap).NetworkValueFor(type);
                //if(value <= 0) continue;
                string label = $"{type.labelShort}: {container.ValueForType(type)}";
                Rect typeRect = new Rect(5, height, 10, 10);
                Vector2 typeSize = Text.CalcSize(label);
                Rect typeLabelRect = new Rect(20, height - 2, typeSize.x, typeSize.y);
                Widgets.DrawBoxSolid(typeRect, type.valueColor);
                Widgets.Label(typeLabelRect, label);
                height += 10 + 2;
            }
            Text.Font = default;
            Text.Anchor = default;
            Widgets.EndGroup();
        }
    }
}
