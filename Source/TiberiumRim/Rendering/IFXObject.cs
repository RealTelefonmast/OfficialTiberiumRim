using System;
using RimWorld;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace TiberiumRim
{
    public interface IFXObject
    {
        ExtendedGraphicData ExtraData { get; }
        float[] OpacityFloats { get; }
        float?[] RotationOverrides { get; }
        float?[] AnimationSpeeds { get; }
        bool[] DrawBools { get; }
        Color[] ColorOverrides { get; }
        Vector3[] DrawPositions { get; }
        Action<FXGraphic>[] Actions { get; }
        Vector2? TextureOffset { get; }
        Vector2? TextureScale { get; }
        bool ShouldDoEffecters  { get; }
        CompPower ForcedPowerComp { get; }
        //CompFX FXComp { get; set; }
        //public int TotalLayers { get; set; }
    }

    public enum FXPropertyEnum
    {
        Opacity
    }

    public struct FXProperties
    {
        private float[] OpacityFloats;
        private float?[] RotationOverrides;
        private float?[] AnimationSpeeds;

        private bool[] DrawBools;

        private Color[] ColorOverrides;

        private Vector3[] DrawPositions;

        private Vector2? TextureOffset;
        private Vector2? TextureScale;

        private Action<FXGraphic>[] Actions;
        private bool ShouldDoEffecters;

        public FXProperties(int size)
        {
            OpacityFloats = new float[size];
            RotationOverrides = new float?[size];
            AnimationSpeeds = new float?[size];
            DrawBools = new bool[size];
            ColorOverrides = new Color[size];
            DrawPositions = new Vector3[size];
            TextureOffset = Vector2.zero;
            TextureScale = Vector2.one;
            Actions = new Action<FXGraphic>[size];
            ShouldDoEffecters = false;
        }

        public void SetEffecters(bool effecterState)
        {
            ShouldDoEffecters = effecterState;
        }

        public void SetValue(FXPropertyEnum type, int index, object value)
        {
            switch (type)
            {
                
            }
        }
    }
}
