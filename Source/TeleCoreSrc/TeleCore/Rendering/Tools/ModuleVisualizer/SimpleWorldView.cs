using RimWorld.Planet;
using TeleCore.Logging;
using TeleCore.Utils;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace TeleCore.Rendering.Tools.ModuleVisualizer;

public class SimpleWorldView
{
    private SimpleWorldCameraDriver cameraDriver;

    private readonly RenderTexture cameraViewTexture;

    //Data

    //Rendering
    private readonly SimpleWorldLayer_ModuleResult layer;
    private readonly Camera simpleWorldCamera;

    public SimpleWorldView()
    {
        //Data
        Self = this;
        WorldGrid = new SimpleWorldGrid();
        layer = new SimpleWorldLayer_ModuleResult(this);
        TLog.Message($"Grid: {WorldGrid.TilesCount}");

        //Rendering
        cameraViewTexture = new RenderTexture(512, 512, 16, RenderTextureFormat.ARGB32);
        cameraViewTexture.Create();

        simpleWorldCamera = MakeCamera();
        simpleWorldCamera.targetTexture = cameraViewTexture;
    }

    public SimpleWorldGrid WorldGrid { get; }

    public ModuleBase ModuleResult { get; private set; }
    public int Seed { get; private set; } = GenText.StableStringHash("TestWorldSeed");

    public static SimpleWorldView Self { get; set; }

    private Camera MakeCamera()
    {
        var gameObject = new GameObject("SimpleWorldCamera", typeof(Camera));
        gameObject.SetActive(true);
        gameObject.AddComponent<SimpleWorldCameraDriver>();

        cameraDriver = gameObject.GetComponent<SimpleWorldCameraDriver>();
        cameraDriver.WorldGrid = WorldGrid;

        Object.DontDestroyOnLoad(gameObject);
        var component = gameObject.GetComponent<Camera>();
        component.orthographic = false;
        component.cullingMask = 1 << 7;
        component.backgroundColor = WorldCameraManager.SkyColor;
        component.clearFlags = CameraClearFlags.Color;
        component.useOcclusionCulling = true;
        component.renderingPath = RenderingPath.Forward;
        component.nearClipPlane = 2f;
        component.farClipPlane = 1200f;
        component.fieldOfView = 20f;
        component.depth = 2f;

        return component;
    }

    public void Update()
    {
        if (ModuleResult != null)
            layer.Render();
    }

    public void SetResult(ModuleBase result)
    {
        ModuleResult = result;
    }

    public void SetSeed(string seed)
    {
    }

    public void ChangedResult()
    {
        layer.SetDirty();
    }

    public void DrawInRect(Rect rect)
    {
        Verse.Widgets.BeginGroup(rect);
        rect = rect.AtZero();

        Verse.Widgets.DrawTextureFitted(rect, cameraViewTexture, 1);

        if (Mouse.IsOver(rect))
            cameraDriver.DriverOnGUI();

        Verse.Widgets.EndGroup();
    }
}