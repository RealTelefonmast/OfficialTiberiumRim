using System.Runtime.InteropServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

namespace TeleCore.Types.Structs;

/// <summary>
///     Alternative SoA (Structure of Arrays) design for better cache utilization
///     when processing all cells for a single gas type
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public unsafe struct GasGridSlice
{
    public NativeArray<ushort> densities;
    public NativeArray<ushort> pending;
    public NativeArray<ushort> overflow;
    public NativeArray<ushort> metadata;

    public readonly int GridSize => densities.Length;

    /// <summary>
    ///     Process all cells in parallel (Burst-compiled job)
    /// </summary>
    [BurstCompile]
    public void ProcessDissipation(float dissipationRate)
    {
        var densityPtr = (ushort*)densities.GetUnsafePtr();
        var count = densities.Length;

        // This will be auto-vectorized by Burst
        for (var i = 0; i < count; i++)
        {
            var current = densityPtr[i];
            if (current > 0)
            {
                var dissipated = (ushort)math.max(0, current - current * dissipationRate);
                densityPtr[i] = dissipated;
            }
        }
    }
}