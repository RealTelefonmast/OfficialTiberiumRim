using UnityEngine;
using Verse;

namespace TeleCore.TeleUI;

public class FontDef : Def
{
    public string? fromAsset;
    public string? fontPath = null;
    public int size = 12;
    public FontStyle style = FontStyle.Normal;
    
    
    public Font Font => GetFont();

    public Font GetFont()
    {
        if (fromAsset != null)
        {
        }
        //TODO: Look into how dynamic fonts are handled when loading at runtime
        var font = new Font(fontPath);
        return font;
    }
}
