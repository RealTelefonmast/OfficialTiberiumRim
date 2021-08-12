using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;

namespace TiberiumRim.VisualEffects
{
    [StaticConstructorOnStartup]
    public class WeatherOverlay_TiberiumPollution : SkyOverlay
    {
        private static readonly Material ParticleOverlay = MatLoader.LoadMat("Weather/SnowOverlayWorld", -1);
        private static readonly Material FoggyOverlay = MatLoader.LoadMat("Weather/FogOverlayWorld", -1);

        private Material overlayCopy;

        public WeatherOverlay_TiberiumPollution()
        {
            //overlayCopy = new Material(Overlay.shader);
            //overlayCopy.CopyPropertiesFromMaterial(Overlay);
            //overlayCopy.color = new Color(0.1f, 0.85f, 0.12f);

            //Overlay.color = new Color(0.1f, 0.85f, 0.12f);

            this.worldOverlayMat = FoggyOverlay;
            this.worldOverlayPanSpeed1 = 0.0005f;
            this.worldOverlayPanSpeed2 = 0.0004f;
            this.worldPanDir1 = new Vector2(1f, 1f);
            this.worldPanDir2 = new Vector2(1f, -1f);
        }

        public void UpdateMaterial(float pollution)
        {
            //var col = overlayCopy.GetColor("_Color");
            //col.a = pollution;
            //overlayCopy.SetColor("_Color", col);
        }
    }
}
