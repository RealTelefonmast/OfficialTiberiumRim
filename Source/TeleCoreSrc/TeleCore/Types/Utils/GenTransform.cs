using System;
using TeleCore.UI;
using UnityEngine;

namespace TeleCore.Types.Utils;

public static class GenTransform
{
    public static Rect OffSetByCoordAnchor(Rect referenceRect, Rect rectToOffset, TexCoordAnchor anchor)
    {
        var rect = rectToOffset;
        var inRect = referenceRect;
        switch (anchor)
        {
            case TexCoordAnchor.Center:
                rect.x = inRect.x + inRect.width / 2f - rect.width / 2f;
                rect.y = inRect.y + inRect.height / 2f - rect.height / 2f;
                break;
            case TexCoordAnchor.Top:
                rect.x = inRect.x + inRect.width / 2f - rect.width / 2f;
                rect.y = inRect.y;
                break;
            case TexCoordAnchor.Bottom:
                rect.x = inRect.x + inRect.width / 2f - rect.width / 2f;
                rect.y = inRect.yMax - rect.height;
                break;
            case TexCoordAnchor.Left:
                rect.x = inRect.x;
                rect.y = inRect.y + inRect.height / 2f - rect.height / 2f;
                break;
            case TexCoordAnchor.Right:
                rect.x = inRect.xMax - rect.width;
                rect.y = inRect.y + inRect.height / 2f - rect.height / 2f;
                break;
            case TexCoordAnchor.TopLeft:
                rect.x = inRect.x;
                rect.y = inRect.y;
                break;
            case TexCoordAnchor.TopRight:
                rect.x = inRect.xMax - rect.width;
                rect.y = inRect.y;
                break;
            case TexCoordAnchor.BottomLeft:
                rect.x = inRect.x;
                rect.y = inRect.yMax - rect.height;
                break;
            case TexCoordAnchor.BottomRight:
                rect.x = inRect.xMax - rect.width;
                rect.y = inRect.yMax - rect.height;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(anchor), anchor, null);
        }

        return rect;
    }

    public static Vector3 OffSetByCoordAnchor(Vector2 referenceSize, Vector2 meshSize, TexCoordAnchor anchor)
    {
        switch (anchor)
        {
            case TexCoordAnchor.Center:
                break;
            case TexCoordAnchor.Top:
                return new Vector3(0, 0, referenceSize.y / 2 - meshSize.y / 2);
            case TexCoordAnchor.Bottom:
                break;
            case TexCoordAnchor.Left:
                break;
            case TexCoordAnchor.Right:
                break;
            case TexCoordAnchor.TopLeft:
                break;
            case TexCoordAnchor.TopRight:
                break;
            case TexCoordAnchor.BottomLeft:
                break;
            case TexCoordAnchor.BottomRight:
                break;
            default:
                return new Vector3(0, 0, 0);
        }

        return new Vector3(0, 0, 0);
    }
}