using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace TeleCore.RimWorld.GasGrid;

/// <summary>
///     Burst-compiled job for gas spreading/diffusion
/// </summary>
[BurstCompile(CompileSynchronously = true)]
public struct GasSpreadingJob : IJob
{
    public NativeArray<ushort> densities;
    public NativeArray<ushort> pending;

    [ReadOnly]
    public NativeArray<byte> passability; // 0-255 passability per cell

    [ReadOnly]
    public int mapSizeX;

    [ReadOnly]
    public int mapSizeZ;

    [ReadOnly]
    public float viscosity;

    [ReadOnly]
    public int minSpreadDensity;

    public void Execute()
    {
        int cellCount = mapSizeX * mapSizeZ;

        // Clear pending
        for (var i = 0; i < cellCount; i++)
            pending[i] = 0;

        // Calculate spreading
        for (var z = 0; z < mapSizeZ; z++)
        for (var x = 0; x < mapSizeX; x++)
        {
            int idx = z * mapSizeX + x;
            ushort density = densities[idx];

            if (density < minSpreadDensity) continue;

            // Check 4 cardinal neighbors
            ProcessNeighbor(idx, x + 1, z, density);
            ProcessNeighbor(idx, x - 1, z, density);
            ProcessNeighbor(idx, x, z + 1, density);
            ProcessNeighbor(idx, x, z - 1, density);
        }

        // Apply pending changes
        for (var i = 0; i < cellCount; i++)
        {
            int newDensity = densities[i] + pending[i];
            densities[i] = (ushort)math.clamp(newDensity, 0, ushort.MaxValue);
        }
    }

    private void ProcessNeighbor(int fromIdx, int nx, int nz, ushort fromDensity)
    {
        if (nx < 0 || nx >= mapSizeX || nz < 0 || nz >= mapSizeZ)
            return;

        int toIdx = nz * mapSizeX + nx;

        // Check passability
        float pass = passability[toIdx] / 255f;
        if (pass <= 0) return;

        ushort toDensity = densities[toIdx];

        // Calculate flow
        float diff = (fromDensity - toDensity) * viscosity * pass;
        if (diff <= 0) return;

        var transfer = (ushort)(diff * 0.25f); // Divide by 4 directions

        // Use atomic operations for thread safety (if using IJobParallelFor)
        pending[fromIdx] -= transfer;
        pending[toIdx] += transfer;
    }
}