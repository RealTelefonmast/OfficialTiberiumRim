using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public static class TRWidgets
    {

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
            GUI.color = TRMats.GapLineColor;
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


    }
}
