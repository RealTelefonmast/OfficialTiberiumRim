using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace TiberiumRim
{
    public class SimpleWorldView
    {
        //Data
        private SimpleWorldGrid grid;

        //Rendering
        private SimpleWorldLayer_ModuleResult layer;
        private Camera simpleWorldCamera;
        private SimpleWorldCameraDriver cameraDriver;
        private RenderTexture cameraViewTexture;

        public SimpleWorldGrid WorldGrid => grid;
        public ModuleBase ModuleResult { get; private set; }
        public int Seed { get; private set; } = GenText.StableStringHash("TestWorldSeed");

        public static SimpleWorldView Self { get; set; }

        public SimpleWorldView()
        {
            //Data
            Self = this;
            grid = new SimpleWorldGrid();
            layer = new SimpleWorldLayer_ModuleResult(this);
            TLog.Message($"Grid: {WorldGrid.TilesCount}");

            //Rendering
            cameraViewTexture = new RenderTexture(512, 512, 16, RenderTextureFormat.ARGB32);
            cameraViewTexture.Create();

            simpleWorldCamera = MakeCamera();
            simpleWorldCamera.targetTexture = cameraViewTexture;
        }

        private Camera MakeCamera()
        {
            GameObject gameObject = new GameObject("SimpleWorldCamera", typeof(Camera));
            gameObject.SetActive(true);
            gameObject.AddComponent<SimpleWorldCameraDriver>();

            cameraDriver = gameObject.GetComponent<SimpleWorldCameraDriver>();
            cameraDriver.WorldGrid = grid;

            UnityEngine.Object.DontDestroyOnLoad(gameObject);
            Camera component = gameObject.GetComponent<Camera>();
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
            if(ModuleResult != null)
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
            GUI.BeginGroup(rect);
            rect = rect.AtZero();

            Widgets.DrawTextureFitted(rect, cameraViewTexture, 1);

            if(Mouse.IsOver(rect))
                cameraDriver.DriverOnGUI();

            GUI.EndGroup();
        }
    }
}
