using System;
using RimWorld;
using TeleCore;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace TiberiumRim
{
    public static class TRWidgets
    {
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

        public static void DrawTexture(float x, float y, float width, Texture2D texture, out float height)
        {
            Vector2 dimensions = new Vector2(texture.width, texture.height);
            float mainPct = dimensions.x / width;
            dimensions /= mainPct;
            height = dimensions.y;
            Rect rect = new Rect(x, y, dimensions.x, dimensions.y);
            Widgets.DrawTextureFitted(rect, texture, 1f);
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
    }
}
