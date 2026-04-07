using UnityEngine;

namespace TeleCore.TeleUI;

public struct RectLayout
{
    private Vector2 _pos;
    private Vector2 _size;
    
    public static RectLayout Begin(Vector2 pos)
    {
        return new RectLayout { _pos = pos };
    }

    public void SetHeight(float height)
    {
        _size.y = height;
    }
    
    public void SetWidth(float width)
    {
        _size.x = width;
    }
}