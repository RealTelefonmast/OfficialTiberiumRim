using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    internal class RoomOverlay_Atmospheric : RoomOverlayRenderer
    {

        [TweakValue("DrawPollution_FlowSpeed", 0f, 2f)]
        public static float FlowSpeed = 0.38f;

        [TweakValue("DrawPollution_BlendSpeed", 0f, 2f)]
        public static float BlendSpeed = 0.4f;

        [TweakValue("DrawPollution_BlendValue", 0f, 1f)]
        public static float BlendValue = 0.48f;

        [TweakValue("DrawPollution_Alpha", 0f, 1f)]
        public static float Alpha = 1f;

        [TweakValue("DrawPollution_Override", 0, 1)]
        public static int Override = 0;

        protected override void InitShaderProps(Material material)
        {
            base.InitShaderProps(material);
            material.SetTexture("_MainTex1", TiberiumContent.Nebula1);
            material.SetTexture("_MainTex2", TiberiumContent.Nebula2);
            material.SetColor("_Color", new ColorInt(0, 255, 97, 255).ToColor);

        }

        protected override void UpdateShaderProps(Material material)
        {
            base.UpdateShaderProps(material);
            material.SetFloat("_BlendValue", BlendValue);
            material.SetFloat("_BlendSpeed", BlendSpeed);
            material.SetFloat("_Opacity", MainAlpha * Alpha);

        }
    }
}
