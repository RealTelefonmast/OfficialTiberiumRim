using UnityEngine;
using Verse;

namespace TeleCore.RWDevTools;

internal class Window_TextureMetaData : Window
{

    public Window_TextureMetaData()
    {
        forcePause = true;
        absorbInputAroundWindow = true;
        layer = WindowLayer.Super;
    }

    public sealed override Vector2 InitialSize => new(UI.screenWidth, UI.screenHeight);
    public override float Margin => 5f;

    public override void PreOpen()
    {
        base.PreOpen();
    }

    public override void DoWindowContents(Rect inRect)
    {
    }

    private void DoSettings(Texture2D texture, Texture tex)
    {
        // texture.filterMode;
        // texture.format;
        // texture.anisoLevel;
        // texture.wrapMode;
        
        //Use MipMap
    }
}