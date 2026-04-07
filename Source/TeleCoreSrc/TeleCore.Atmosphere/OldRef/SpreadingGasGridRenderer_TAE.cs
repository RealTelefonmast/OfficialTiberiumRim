// Preserved from TeleCore/SpreadingGas/SpreadingGasGridRenderer.cs
using System.Runtime.InteropServices;
using TeleCore;
using TeleCore.Loading;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using Verse;

namespace TeleCore.Atmosphere.OldRef
{
    [StructLayout(LayoutKind.Sequential)]
    struct GasMeshProperties_TAE
    {
        public uint forwardIndex;    //Only used to forward to a different MeshProp data struct
        public uint mapIndex;        //CellIndex on the Map
        public Matrix4x4 _matrix;

        public static int Size()
        {
            return Marshal.SizeOf<GasMeshProperties_TAE>();
        }
    }

    public class SpreadingGasGridRenderer_TAE
    {
        private TAE.SpreadingGasGrid grid;
        private readonly Map map;
        private int bufferSize;

        private uint[] shaderArguments = new uint[5];

        //
        private Material _material;
        private float angle;
        private static Bounds bounds = new(Vector3.zero, Vector3.one * 10000f);

        //Buffer Data
        private NativeArray<GasMeshProperties_TAE> meshProperties;
        private unsafe GasMeshProperties_TAE* meshPropsPtr;

        //
        private NativeArray<uint> indexedDensities;
        private unsafe uint* indexedDensitiesPtr;

        private NativeArray<Matrix4x4> internalMatrices;
        private unsafe Matrix4x4* internalMatricesPtr;

        private ComputeBuffer bufferMinColors;
        private ComputeBuffer bufferMaxColors;
        private ComputeBuffer bufferMaxDensities;

        private ComputeBuffer bufferMeshData;
        private ComputeBuffer bufferIndexedDensities;
        private ComputeBuffer bufferArguments;

        private bool isInitialised = false;

        //PropertyIDs
        private const string PropertyTypeCount = "_TypeCount";
        private const string PropertyMaxAlpha = "_MaxAlpha";
        private const string PropertyRotSpeed = "_RotSpeed";
        private const string PropertySrcMode = "_SrcMode";
        private const string PropertyDstMode = "_DstMode";
        private const string PropertyBufferMinColors = "_MinColors";
        private const string PropertyBufferMaxColors= "_MaxColors";
        private const string PropertyBufferMeshProps = "_MeshProperties";

        public SpreadingGasGridRenderer_TAE(TAE.SpreadingGasGrid grid, Map map)
        {
            this.grid = grid;
            this.map = map;
            this.bufferSize = map.cellIndices.NumGridCells;

            SetupInternalMatrixBuffer();

            void Unloader()
            {
                LongEventHandler.ExecuteWhenFinished(delegate
                {
                    meshProperties.Dispose();
                    indexedDensities.Dispose();

                    bufferMinColors?.Dispose();
                    bufferMaxColors?.Dispose();
                    bufferMaxDensities?.Dispose();

                    bufferMeshData?.Dispose();
                    bufferIndexedDensities?.Dispose();
                    bufferArguments?.Dispose();
                });
            }

            UnloadUtility.RegisterUnloadAction(Unloader);
            ApplicationQuitUtility.RegisterQuitEvent(Unloader);
        }

        [TweakValue("Atmospheric", 0, 1000)]
        private static float _RotSpeed = 100;

        [TweakValue("Atmospheric", 0, 1)]
        private static float _MaxAlpha = 1;

        [TweakValue("Atmospheric", 0, 1)]
        private static float _MinAlpha = 0.0625f;

        private unsafe void SetupInternalMatrixBuffer()
        {
            internalMatrices = new NativeArray<Matrix4x4>(bufferSize, Allocator.Persistent);
            internalMatricesPtr = (Matrix4x4*)internalMatrices.GetUnsafePtr();

            var rad = Rand.Range(2.75f, 3.75f);
            Vector3 size = new(rad, 0f, rad);
            for (int i = 0; i < bufferSize; i++)
            {
                Rand.PushState(i);
                Vector3 pos = map.cellIndices.IndexToCell(i).ToVector3ShiftedWithAltitude(AltitudeLayer.Gas);
                pos.x += Rand.Range(-0.25f, 0.25f);
                pos.z += Rand.Range(-0.24f, 0.24f);
                Quaternion rotation = Quaternion.AngleAxis(((float) Rand.Range(0, 360)), Vector3.up);
                internalMatricesPtr[i] = Matrix4x4.TRS(pos, rotation, size);
                Rand.PopState();
            }
        }

        public void Draw()
        {
            if (!grid.HasAnyGas) return;

            if (!isInitialised)
            {
                isInitialised = true;
            }

            if(Find.TickManager.TicksGame % 2 == 0)
                UpdateGPUData();

            Graphics.DrawMeshInstancedIndirect(MeshPool.plane10, 0, null, bounds, bufferArguments);
        }

        private void UpdateGPUData()
        {
            UpdateArguments();
            UpdateMeshProps();
        }

        private void UpdateArguments()
        {
            shaderArguments[0] = MeshPool.plane10.GetIndexCount(0);
            shaderArguments[1] = grid.TotalGasCount;
            shaderArguments[2] = MeshPool.plane10.GetIndexStart(0);
            shaderArguments[3] = 0;
            bufferArguments.SetData(shaderArguments);
        }

        private unsafe void UpdateMeshProps()
        {
            int j = 0;
            for (uint i = 0; i < grid.GasGrid.Length; i++)
            {
                if (!grid.AnyGasAtUnsafe(i)) continue;

                var forwarded = meshPropsPtr[j];
                forwarded.forwardIndex = i;
                meshPropsPtr[j] = forwarded;

                var meshProps = meshPropsPtr[i];
                meshProps._matrix = internalMatricesPtr[i];
                meshProps.mapIndex = i;
                meshPropsPtr[i] = meshProps;
                grid.AddDensities(indexedDensitiesPtr, i);

                j++;
            }

            bufferMeshData.SetData(meshProperties);
            bufferIndexedDensities.SetData(indexedDensities);
        }
    }
}
