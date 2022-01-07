using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using Verse;

namespace TiberiumRim
{
    public struct GlowerSource
    {
        public uint index;
        public uint active;
        public float radius;
        public float overlightRadius;
        public Color color;

        public GlowerSource(uint index)
        {
            this.index = index;
            active = 0;
            radius = 0;
            overlightRadius = 0;
            color = Color.clear;
        }

        public GlowerSource(uint index, uint active, float radius, float overlightRadius, Color color)
        {
            this.index = index;
            this.active = active;
            this.radius = radius;
            this.overlightRadius = overlightRadius;
            this.color = color;
        }
    }

    public static class GPUTools
    {
        
        private static FilterMode defaultFilterMode = FilterMode.Bilinear;
        private static GraphicsFormat defaultGraphicsFormat = GraphicsFormat.R16G16B16A16_SFloat;

        public static void CreateRenderTexture(ref RenderTexture texture, int width, int height)
        {
            CreateRenderTexture(ref texture, width, height, defaultFilterMode);
        }

        public static void CreateRenderTexture(ref RenderTexture texture, int width, int height, FilterMode filterMode)
        {
            if (texture == null || !texture.IsCreated() || texture.width != width || texture.height != height)
            {
                if (texture != null)
                {
                    texture.Release();
                }
                texture = new RenderTexture(width, height, 0);
                //texture.graphicsFormat = format;
                texture.enableRandomWrite = true;

                texture.autoGenerateMips = false;
                texture.Create();
            }
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = filterMode;
        }

        /// Copy the contents of one render texture into another. Assumes textures are the same size.
        public static void CopyRenderTexture(Texture source, RenderTexture target)
        {
            Graphics.Blit(source, target);
        }
    }

    public class GlowGridGPUInfo : MapInformation
    {
        public GlowGridGPUInfo(Map map) : base(map)
        {
            //glowGrid = new ComputeGrid<Color>(map, (_) => Color.clear);
            //glowSources = new ComputeGrid<GlowerSource>(map, c => new GlowerSource((uint)c));
        }

        /*
        //Shader Data
        private ComputeShader shaderInt;
        private ComputeGrid<Color> glowGrid;
        private ComputeGrid<GlowerSource> glowSources;
        private RenderTexture renderTextureShader;
        private RenderTexture renderTextureIngame;

        public static Material renderMaterial;

        private ComputeShader MainShader
        {
            get
            {
                if (shaderInt == null)
                {
                    var dynamic = Map.Tiberium().DynamicDataInfo;
                    Log.Message($"Setting up GlowShader {glowGrid.IsReady} | {glowSources.IsReady} # {dynamic.LightPassGrid.IsReady} | {dynamic.EdificeGrid.IsReady}");

                    shaderInt = TRContentDatabase.GlowFlooderCompute;
                    GPUTools.CreateRenderTexture(ref renderTextureShader, map.Size.x, map.Size.z, FilterMode.Bilinear);
                    GPUTools.CreateRenderTexture(ref renderTextureIngame, map.Size.x, map.Size.z, FilterMode.Bilinear);

                    shaderInt.SetTexture(0, "Result", renderTextureShader);
                    shaderInt.SetBuffer(0, "glowGrid", glowGrid.DataBuffer);
                    shaderInt.SetBuffer(0, "glowSources", glowSources.DataBuffer);

                    //Metadata
                    shaderInt.SetBuffer(0, "gameGlowGrid", dynamic.LightPassBuffer);
                    shaderInt.SetBuffer(0, "blockerGrid", dynamic.EdificeBuffer);

                    shaderInt.SetInts("MAP_SIZE", map.Size.x, map.Size.y, map.Size.z);
                    shaderInt.SetInt("LENGTH", map.cellIndices.NumGridCells);

                    renderMaterial = new Material(ShaderDatabase.MoteGlow);
                    renderMaterial.mainTexture = renderTextureIngame;
                }

                return shaderInt;
            }
        }

        public override void SafeInit()
        {
            glowGrid.ThreadSafeInit();
            glowSources.ThreadSafeInit();
            Updater();
        }

        public void Notify_AddGlower(CompGlower glower)
        {
            glowSources.SetValue(glower.parent.Position, new GlowerSource((uint)glower.parent.Position.Index(Map), 1, glower.Props.glowRadius, glower.Props.overlightRadius, glower.Props.glowColor.ToColor));
            Updater();

            GPUTools.CopyRenderTexture(renderTextureShader, renderTextureIngame);
            
        }

        public void Notify_RemoveGlower(CompGlower glower)
        {
            glowSources.ResetValue(glower.parent.Position, new GlowerSource((uint)glower.parent.Position.Index(Map)));
            Updater();

            GPUTools.CopyRenderTexture(renderTextureShader, renderTextureIngame);
        }

        private void Updater()
        {
            if (glowGrid.IsReady)
            {
                Log.Message("UPDATING GLOWGRID");
                MainShader.Dispatch(0, glowGrid.Length, 1, 1);
                glowGrid.UpdateCPUData();

                
                Color[] colors = new Color[map.cellIndices.NumGridCells];
                Color[] colors2 = new Color[map.cellIndices.NumGridCells];
                IntVec2 size = new IntVec2(map.Size.x, map.Size.z);
                for (int i = 0; i < colors.Length; i++)
                {
                    colors[i] = glowSources[i].color;
                }

                //Array.Copy(glowGrid.Grid, colors2, colors2.Length);
                //TiberiumContent.GenerateTextureFrom(colors2, size, $"GPU_GlowGrid");
                //TiberiumContent.GenerateTextureFrom(colors, size, "GlowSources");
                
            }
        }
        */
    }
}
