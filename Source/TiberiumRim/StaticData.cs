using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    [StaticConstructorOnStartup]
    public static class StaticData
    {
        public static Dictionary<int, MapComponent_Tiberium> TiberiumMapComp;
        public static Dictionary<int, RenderTexture> FlowMapsByMap;

        public static Dictionary<int, Color[]> CanvasBySize;


        static StaticData()
        {
            Notify_Reload();
        }

        public static void Notify_Reload()
        {
            Log.Message("Clearing StaticData!");
            CanvasBySize = new Dictionary<int, Color[]>();

            TiberiumMapComp = new Dictionary<int, MapComponent_Tiberium>();
            FlowMapsByMap = new Dictionary<int, RenderTexture>();
        }

        public static void Notify_NewTibMapComp(MapComponent_Tiberium mapComp)
        {
            TiberiumMapComp[mapComp.map.uniqueID] = mapComp;
            var map = mapComp.map;
            var pixelDensity = TiberiumContent.FlowMapPixelDensity;
            FlowMapsByMap[mapComp.map.uniqueID] = new RenderTexture(pixelDensity * map.Size.x, pixelDensity * map.Size.z, 0);
        }

        public static Color[] GetCanvasFor(int size)
        {
            return CanvasBySize[size];
        }

        public static RenderTexture FlowMapTextureFor(int id)
        {
            return FlowMapsByMap[id];
        }

        public static Color[] PixelCanvas(int size, int pixelDensity)
        {
            var newCanvas = new Color[size * size * pixelDensity * pixelDensity];
            for (int i = 0; i < newCanvas.Length; i++)
            {
                newCanvas[i] = Color.clear;
            }
            if (CanvasBySize.TryGetValue(size, out _))
            {
                CanvasBySize.Remove(size);
            }
            CanvasBySize.Add(size, newCanvas);
            return newCanvas;
        }

        public static void AddToFlowMap(Map map, Color[] pixelData)
        {
            var texture = FlowMapsByMap[map.uniqueID];
            RenderTexture.active = texture;


            RenderTexture.active = null;
        }
    }
}
