using UnityEngine;

namespace TeleCore.Static;

public static class TeleShaderIDs
{
    public static readonly int CameraZoom = Shader.PropertyToID("_CameraZoom");
    public static readonly int WindSpeed = Shader.PropertyToID("_WindSpeed");
    public static readonly int OffsetID = Shader.PropertyToID("_Offset");
}