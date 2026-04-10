using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace TeleCore.SDGUI;

public partial class UIElement
{
    private Vector2 _pos;
    private Vector2 _size;
    
    public Rect Rect => new Rect(_pos, _size);
    public int ID { get; private set; }

    public UIElement(Rect rect) : this(rect.position, rect.size)
    {
    }
    
    public UIElement(Vector2 pos, Vector2 size)
    {
        _pos = pos;
        _size = size;
    }
    
    public void Draw()
    {
        //Set ID
        ID = GUIUtility.GetControlID(FocusType.Passive, Rect);
        var ev = Event.current;
        var evType = ev.type;

        switch (evType)
        {
            case EventType.Layout:
            {
                LayoutInt();
                break;
            }
            case EventType.Repaint:
            {
                RepaintInt();
                break;
            }
            case EventType.MouseDown:
            {
                MouseDownInt();
                break;
            }
            case EventType.MouseUp:
            {
                MouseUpInt();
                break;
            }
        }
    }
}