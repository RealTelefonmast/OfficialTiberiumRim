using UnityEngine;

namespace TeleCore.DGUI;

public class ScrollableGrid : UIElement
{
    private Vector2 _gridPos;
    private float _zoom;

    public override void Draw()
    {
        //Draw Grid
        base.Draw();
    }
}

public class GridMaterial
{
    private Material _mat;

    public void Draw(Vector2 pos, float zoom)
    {
        _mat.SetVector("_GridPos", pos);
        _mat.SetFloat("_Zoom", zoom);
    }
}