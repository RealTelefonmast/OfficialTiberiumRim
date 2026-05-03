using UnityEngine;

namespace TeleCore.Unsorted;

public static class TextureUtils
{
    public static Texture2D CopyReadable(Texture2D source)
    {
        var temporary = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.Default,
            RenderTextureReadWrite.Linear);
        temporary.name = source.name;
        Graphics.Blit(source, temporary);
        var active = RenderTexture.active;
        RenderTexture.active = temporary;
        var texture2D = new Texture2D(source.width, source.height);
        texture2D.ReadPixels(new Rect(0f, 0f, temporary.width, temporary.height), 0, 0);
        texture2D.Apply();
        RenderTexture.active = active;
        RenderTexture.ReleaseTemporary(temporary);
        return texture2D;
    }
}