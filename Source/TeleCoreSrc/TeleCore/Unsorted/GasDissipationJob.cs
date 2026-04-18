using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace TeleCore.Unsorted;

/// <summary>
/// Burst-compiled job for parallel gas dissipation
/// </summary>
[BurstCompile(CompileSynchronously = true)]
public struct GasDissipationJob : IJobParallelFor
{
    [ReadOnly] public float dissipationRate;
    [ReadOnly] public float deltaTime;
    [ReadOnly] public int gasTypeId;
        
    // Gas data for this specific gas type (SoA layout)
    public NativeArray<ushort> densities;
    [WriteOnly] public NativeArray<ushort> pendingChanges;
        
    // Map data
    [ReadOnly] public NativeArray<bool> roofedCells;
    [ReadOnly] public int mapSizeX;
        
    public void Execute(int index)
    {
        var density = densities[index];
        if (density == 0) return;
            
        // Calculate dissipation based on roof
        float dissipation = dissipationRate * deltaTime;
        if (roofedCells[index])
            dissipation *= 0.5f; // Slower dissipation under roof
            
        var reduction = (ushort)math.min((int)density, (ushort)(density * dissipation));
        pendingChanges[index] = reduction;
    }
}