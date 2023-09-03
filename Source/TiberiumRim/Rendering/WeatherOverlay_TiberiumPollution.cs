using UnityEngine;
using Verse;

namespace TR
{

    [StaticConstructorOnStartup]
    public class WeatherOverlay_TiberiumPollution : SkyOverlay
    {
        private static readonly Material ParticleOverlay = MatLoader.LoadMat("Weather/SnowOverlayWorld", -1);
        private Material materialCopy;

        private Material MainMat => materialCopy;

        public WeatherOverlay_TiberiumPollution()
        {
            materialCopy = new Material(ParticleOverlay.shader);
            materialCopy.CopyPropertiesFromMaterial(ParticleOverlay);

            this.worldOverlayMat = MainMat;
            this.worldOverlayPanSpeed1 = 0.0008f;
            this.worldPanDir1 = new Vector2(-0.25f, -1f);
            this.worldPanDir1.Normalize();
            this.worldOverlayPanSpeed2 = 0.0012f;
            this.worldPanDir2 = new Vector2(-0.24f, -1f);
            this.worldPanDir2.Normalize();
        }

        //Data
        public void UpdateMaterial(float pollution)
        {
            var col = MainMat.GetColor(ShaderPropertyIDs.Color);
            col.a = pollution;
            MainMat.SetColor(ShaderPropertyIDs.Color, col);
        }
    }

    /*
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
    */
}
