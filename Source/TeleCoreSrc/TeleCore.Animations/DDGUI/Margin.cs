namespace TeleCore.DDGUI;

public readonly struct Padding(float left, float right, float top, float bottom)
{
    public readonly float Left = left;
    public readonly float Right = right;
    public readonly float Top = top;
    public readonly float Bottom = bottom;

    public Padding(float x, float y) : this(x, x, y, y)
    {
    }
}

public readonly struct Margin(float left, float right, float top, float bottom)
{
    public readonly float Left = left;
    public readonly float Right = right;
    public readonly float Top = top;
    public readonly float Bottom = bottom;

    public Margin(float x, float y) : this(x, x, y, y)
    {
    }
}
