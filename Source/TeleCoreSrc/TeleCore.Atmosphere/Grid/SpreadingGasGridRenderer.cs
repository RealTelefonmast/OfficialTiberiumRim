using System.Runtime.InteropServices;
using LudeonTK;
using TeleCore.Absorbed.RWLib.Patches;
using TeleCore.Atmosphere.Static;
using TeleCore.Logging;
using TeleCore.Loader;
using TeleCore.Utils;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using Verse;

namespace TeleCore.Atmosphere.Grid;

[StructLayout(LayoutKind.Sequential)]
struct GasMeshProperties
{
    public uint forwardIndex;    //Only used to forward to a different MeshProp data struct
    public uint mapIndex;        //CellIndex on the Map
    public Matrix4x4 _matrix;

    public static int Size()
    {
        return Marshal.SizeOf<GasMeshProperties>();
    }
}

public class SpreadingGasGridRenderer
{
    private SpreadingGasGrid grid;
    private readonly Map map;
    private int bufferSize;

    private uint[] shaderArguments = new uint[5];

    //
    private Material _material;
    private float angle;
    private static Bounds bounds = new(Vector3.zero, Vector3.one * 10000f);

    //Buffer Data
    private NativeArray<GasMeshProperties> meshProperties;
    private unsafe GasMeshProperties* meshPropsPtr;

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

    //
    private const string PropertySrcMode = "_SrcMode";
    private const string PropertyDstMode = "_DstMode";

    //
    private const string PropertyBufferMinColors = "_MinColors";
    private const string PropertyBufferMaxColors= "_MaxColors";

    private const string PropertyBufferMeshProps = "_MeshProperties";


    public SpreadingGasGridRenderer(SpreadingGasGrid grid, Map map)
    {
        this.grid = grid;
        this.map = map;
        this.bufferSize = map.cellIndices.NumGridCells;

        //
        SetupInternalMatrixBuffer();

        //
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

    public Material Material
    {
        get
        {
            if (_material == null)
            {
                if (TAEUnityContent.InstancedGas == null)
                {
                    TLog.Error($"Cannot find {nameof(TAEUnityContent.InstancedGas)}!");
                }

                _material = MaterialPool.MatFrom("Sprites/Gas", TAEUnityContent.InstancedGas, 3170);
                _material.SetInt(PropertyTypeCount, SpreadingGasGrid.GasDefsCount);

            }
            return _material;
        }
    }

    [TweakValue("Atmospheric", 0, 1000)]
    private static float _RotSpeed = 100;

    [TweakValue("Atmospheric", 0, 1)]
    private static float _MaxAlpha = 1;

    [TweakValue("Atmospheric", 0, 1)]
    private static float _MinAlpha = 0.0625f;

    public void UpdateGPUData()
    {
        //Set Mesh Data
        UpdateArguments();
        UpdateMeshProps();

        //Set Speed
        Material.SetFloat(PropertyRotSpeed, _RotSpeed);
        Material.SetFloat("_MinAlpha", _MinAlpha);
        Material.SetFloat("_MaxAlpha", _MaxAlpha);
    }

    //
    private unsafe void SetupInternalMatrixBuffer()
    {
        internalMatrices = new NativeArray<Matrix4x4>(bufferSize, Allocator.Persistent); //new Matrix4x4[bufferSize];
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

    private void InitColorBuffers()
    {
        bufferMinColors = new ComputeBuffer(SpreadingGasGrid.GasDefsCount, Marshal.SizeOf<Color>());
        bufferMaxColors = new ComputeBuffer(SpreadingGasGrid.GasDefsCount, Marshal.SizeOf<Color>());
        bufferMaxDensities = new ComputeBuffer(SpreadingGasGrid.GasDefsCount, sizeof(uint));

        bufferMinColors.SetData(grid.minColors);
        bufferMaxColors.SetData(grid.maxColors);
        bufferMaxDensities.SetData(grid.maxDensities);

        Material.SetBuffer(PropertyBufferMinColors, bufferMinColors);
        Material.SetBuffer(PropertyBufferMaxColors, bufferMaxColors);
        Material.SetBuffer("_MaxDensities", bufferMaxDensities);
    }

    private unsafe void InitRenderData()
    {
        //
        InitColorBuffers();

        //
        meshProperties = new NativeArray<GasMeshProperties>(bufferSize, Allocator.Persistent);
        indexedDensities = new NativeArray<uint>(bufferSize * SpreadingGasGrid.GasDefsCount, Allocator.Persistent);

        meshPropsPtr = (GasMeshProperties*)meshProperties.GetUnsafePtr();
        indexedDensitiesPtr = (uint*)indexedDensities.GetUnsafePtr();

        //
        bufferMeshData = new ComputeBuffer(bufferSize, GasMeshProperties.Size(), ComputeBufferType.Structured);
        bufferIndexedDensities = new ComputeBuffer(indexedDensities.Length, sizeof(uint), ComputeBufferType.Structured);
        bufferArguments = new ComputeBuffer(1, shaderArguments.Length * sizeof(uint), ComputeBufferType.IndirectArguments);

        //
        Material.SetFloat(PropertyMaxAlpha, 1.0f);
        Material.SetBuffer(PropertyBufferMeshProps, bufferMeshData);
        Material.SetBuffer("_IndexedDensities", bufferIndexedDensities);
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
        //Fill
        int j = 0;
        for (uint i = 0; i < grid.Length; i++)
        {
            if (!grid.AnyGasAtUnsafe(i)) continue;

            //Map internalIndex j of GPU instance, to mapIndex i
            var forwarded = meshPropsPtr[j];
            forwarded.forwardIndex = i;
            meshPropsPtr[j] = forwarded;

            //
            var meshProps = meshPropsPtr[i];
            meshProps._matrix = internalMatricesPtr[i];

            meshProps.mapIndex = i;

            //
            meshPropsPtr[i] = meshProps;
            grid.AddDensities(indexedDensitiesPtr, i);

            j++;
        }

        //TLog.Message($"{indexedAlphas.ToStringSafeEnumerable()}");

        bufferMeshData.SetData(meshProperties);
        bufferIndexedDensities.SetData(indexedDensities);
    }

    //
    public void Draw()
    {
        if (!grid.HasAnyGas) return;

        if (!isInitialised)
        {
            InitRenderData();
            isInitialised = true;
        }

        //
        if(Find.TickManager.TicksGame % 2 == 0)
            UpdateGPUData();

        //Render Always
        Graphics.DrawMeshInstancedIndirect(MeshPool.plane10, 0, Material, bounds, bufferArguments);
    }
}
