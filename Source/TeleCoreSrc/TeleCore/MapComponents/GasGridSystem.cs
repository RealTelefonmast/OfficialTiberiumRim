using System;
using RimWorld;
using TeleCore.Defs;
using TeleCore.Unsorted;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Verse;

namespace TeleCore.MapComponents;

/// <summary>
///     Main gas grid system that manages Burst jobs
/// </summary>
public class GasGridSystem : MapComponent
{
    // Job handles for tracking completion
    private JobHandle dissipationJobHandle;
    private JobHandle spreadingJobHandle;

    // Native arrays for job data
    private NativeArray<ushort>[] gasDensities; // One per gas type
    private NativeArray<ushort>[] gasPending;
    private NativeArray<bool> roofedCells;
    private NativeArray<byte> passabilityGrid;

    // Job instances (reused each frame)
    private GasDissipationJob[] dissipationJobs;
    private GasSpreadingJob[] spreadingJobs;

    // Timing
    private int tickCounter;
    private const int TicksPerProcess = 3; // Process every 3 ticks

    public GasGridSystem(Map map) : base(map)
    {
        // Ensure memory manager is initialized first
        if (!GasGridMemoryManager.IsInitialized) GasGridMemoryManager.Initialize();

        InitializeNativeArrays();
        CreateJobInstances();
    }

    private void InitializeNativeArrays()
    {
        TLog.Debug(
            $"Initializing native arrays on map... for {map.cellIndices.NumGridCells} cells and {GasGridMemoryManager.GasTypeCount} gas types");
        try
        {
            int cellCount = map.cellIndices.NumGridCells;
            int gasCount = GasGridMemoryManager.GasTypeCount;

            // Validate parameters
            if (cellCount <= 0)
            {
                TLog.Error($"Invalid cell count: {cellCount}");
                return;
            }

            if (gasCount <= 0)
            {
                TLog.Error($"Invalid gas count: {gasCount}. GasGridMemoryManager may not be initialized.");
                return;
            }

            // Allocate native arrays
            gasDensities = new NativeArray<ushort>[gasCount];
            gasPending = new NativeArray<ushort>[gasCount];

            for (var i = 0; i < gasCount; i++)
            {
                gasDensities[i] = new NativeArray<ushort>(cellCount, Allocator.Persistent);
                gasPending[i] = new NativeArray<ushort>(cellCount, Allocator.Persistent);
            }

            roofedCells = new NativeArray<bool>(cellCount, Allocator.Persistent);
            passabilityGrid = new NativeArray<byte>(cellCount, Allocator.Persistent);

            // Check if arrays were created successfully
            if (!roofedCells.IsCreated || !passabilityGrid.IsCreated)
            {
                TLog.Error("Failed to create NativeArrays");
                return;
            }

            UpdateMapData();
        }
        catch (Exception e)
        {
            TLog.Error($"Failed to initialize GasGridSystem native arrays: {e}");
        }
    }

    private void CreateJobInstances()
    {
        int gasCount = GasGridMemoryManager.GasTypeCount;
        dissipationJobs = new GasDissipationJob[gasCount];
        spreadingJobs = new GasSpreadingJob[gasCount];

        for (var i = 0; i < gasCount; i++)
        {
            var gasDef = GasGridMemoryManager.AllGasDefs[i];

            dissipationJobs[i] = new GasDissipationJob
            {
                dissipationRate = gasDef.dissipationAmount / (float)ushort.MaxValue,
                deltaTime = TicksPerProcess,
                gasTypeId = i,
                densities = gasDensities[i],
                pendingChanges = gasPending[i],
                roofedCells = roofedCells,
                mapSizeX = map.Size.x
            };

            spreadingJobs[i] = new GasSpreadingJob
            {
                densities = gasDensities[i],
                pending = gasPending[i],
                passability = passabilityGrid,
                mapSizeX = map.Size.x,
                mapSizeZ = map.Size.z,
                viscosity = gasDef.spreadViscosity,
                minSpreadDensity = gasDef.minSpreadDensity
            };
        }
    }

    void UpdateMapData()
    {
        // Safety check - ensure arrays are created
        if (!roofedCells.IsCreated || !passabilityGrid.IsCreated)
        {
            Log.Warning("UpdateMapData called but NativeArrays not initialized");
            return;
        }
            
        int cellCount = map.cellIndices.NumGridCells;
            
        // Validate array sizes
        if (roofedCells.Length != cellCount || passabilityGrid.Length != cellCount)
        {
            Log.Error($"Array size mismatch. Expected: {cellCount}, Got: roofed={roofedCells.Length}, pass={passabilityGrid.Length}");
            return;
        }
            
        for (int i = 0; i < cellCount; i++)
        {
            var cell = map.cellIndices.IndexToCell(i);
                
            // Update roof data
            roofedCells[i] = cell.Roofed(map);
                
            // Update passability (0-255 scale)
            var edifice = cell.GetEdifice(map);
            if (edifice == null)
            {
                passabilityGrid[i] = 255; // Fully passable
            }
            else if (edifice.def.fillPercent >= 1f)
            {
                // Check for doors
                if (edifice is Building_Door door)
                    passabilityGrid[i] = (byte)(door.Open ? 255 : 0);
                else
                    passabilityGrid[i] = 0; // Blocked
            }
            else
            {
                // Partial blockage
                passabilityGrid[i] = (byte)((1f - edifice.def.fillPercent) * 255);
            }
        }
    }

    /// <summary>
    ///     Called from MapComponent.MapComponentTick()
    /// </summary>
    public override void MapComponentTick()
    {
        tickCounter++;

        if (tickCounter % TicksPerProcess != 0)
            return;

        // Complete any running jobs
        CompleteJobs();

        // Update map data periodically
        if (tickCounter % 60 == 0)
            UpdateMapData();

        // Schedule new jobs
        ScheduleJobs();
    }

    private void ScheduleJobs()
    {
        int gasCount = GasGridMemoryManager.GasTypeCount;
        var previousHandle = new JobHandle();

        // Schedule dissipation jobs (can run in parallel)
        for (var i = 0; i < gasCount; i++)
        {
            var handle = dissipationJobs[i].Schedule(
                map.cellIndices.NumGridCells,
                64, // Batch size for parallel processing
                previousHandle
            );
            previousHandle = JobHandle.CombineDependencies(previousHandle, handle);
        }

        dissipationJobHandle = previousHandle;

        // Schedule spreading jobs (run sequentially after dissipation)
        for (var i = 0; i < gasCount; i++) previousHandle = spreadingJobs[i].Schedule(previousHandle);

        spreadingJobHandle = previousHandle;
    }

    void CompleteJobs()
    {
        // This blocks until jobs complete
        if (!spreadingJobHandle.IsCompleted)
            spreadingJobHandle.Complete();
            
        // Jobs are done, data is now updated
        // You can now safely read from gasDensities arrays
    }

    /// <summary>
    ///     Add gas at a position (called from main thread)
    /// </summary>
    public void AddGas(IntVec3 pos, int gasTypeId, ushort amount)
    {
        TLog.Debug("Adding gas..");
        // Ensure jobs are complete before modifying
        CompleteJobs();

        int idx = CellIndicesUtility.CellToIndex(pos, map.Size.x);
        var densities = gasDensities[gasTypeId];

        densities[idx] = (ushort)math.min(densities[idx] + amount, ushort.MaxValue);
    }

    /// <summary>
    ///     Get gas density at position (safe to call anytime)
    /// </summary>
    public ushort GetGasDensity(IntVec3 pos, int gasTypeId)
    {
        // Ensure jobs are complete before reading
        CompleteJobs();

        int idx = CellIndicesUtility.CellToIndex(pos, map.Size.x);
        return gasDensities[gasTypeId][idx];
    }

    public void Debug_FillAll()
    {
        throw new NotImplementedException();
    }

    public void Debug_PushRadialAdjacent(IntVec3 mouseCell, SpreadingGasDef def)
    {
        //TODO: Add correct id system later
        TLog.Debug($"Adding gas {def.defName} with id {DefIDStack.ToID(def)} (:0) on tile {CellUtility.Index(mouseCell, map)}");
        AdjacentCellFiller.FillAdjacentCellsAround(mouseCell, map, 128,
            vec3 => { AddGas(vec3, 0, def.maxDensityPerCell); }, (vec3 => true), (vec3 => false));
    }

    public bool AnyGasAt(IntVec3 intVec)
    {
        foreach (var density in gasDensities)
            if (density[CellUtility.Index(intVec, map)] > 0)
                return true;
        return false;
    }
}

/*public class GasGridSystem : MapComponent
{
    private GasCellStack[] grid;

    // BURST TEST
    // Job handles for tracking completion
    private JobHandle dissipationJobHandle;
    private JobHandle spreadingJobHandle;

    // Job instances (reused each frame)
    private GasDissipationJob[] dissipationJobs;
    private GasSpreadingJob[] spreadingJobs;

    // Native arrays for job data
    private NativeArray<ushort>[] gasDensities; // One per gas type
    private NativeArray<ushort>[] gasPending;
    private NativeArray<bool> roofedCells;
    private NativeArray<byte> passabilityGrid;

    // Timing
    private int tickCounter = 0;
    private const int TicksPerProcess = 3; // Process every 3 ticks

    public GasGridSystem(Map map) : base(map)
    {
        TLog.Debug("Making GasGrid System");
        // Create grid with proper memory allocation
        int cellCount = map.cellIndices.NumGridCells;
        grid = new GasCellStack[cellCount];

        for (int i = 0; i < cellCount; i++)
        {
            grid[i] = GasGridMemoryManager.CreateStack();
            TLog.ErrorOnce($"NOT_ERROR: Made gasGridCell: {grid[i]}", 78126348);
        }
    }

    /// <summary>
    /// Called from MapComponent.MapComponentTick()
    /// </summary>
    public override void MapComponentTick()
    {
        tickCounter++;

        if (tickCounter % TicksPerProcess != 0)
            return;

        // Complete any running jobs
        CompleteJobs();

        // Update map data periodically
        if (tickCounter % 60 == 0)
            UpdateMapData();

        // Schedule new jobs
        ScheduleJobs();
    }

    void ScheduleJobs()
    {
        var gasCount = GasGridMemoryManager.GasTypeCount;
        var previousHandle = new JobHandle();

        // Schedule dissipation jobs (can run in parallel)
        for (int i = 0; i < gasCount; i++)
        {
            var handle = dissipationJobs[i].Schedule(
                map.cellIndices.NumGridCells,
                64, // Batch size for parallel processing
                previousHandle
            );
            previousHandle = JobHandle.CombineDependencies(previousHandle, handle);
        }

        dissipationJobHandle = previousHandle;

        // Schedule spreading jobs (run sequentially after dissipation)
        for (int i = 0; i < gasCount; i++)
        {
            previousHandle = spreadingJobs[i].Schedule(previousHandle);
        }

        spreadingJobHandle = previousHandle;
    }

    void CompleteJobs()
    {
        // This blocks until jobs complete
        spreadingJobHandle.Complete();

        // Jobs are done, data is now updated
        // You can now safely read from gasDensities arrays
    }

    void InitializeNativeArrays()
    {
        int cellCount = map.cellIndices.NumGridCells;
        int gasCount = GasGridMemoryManager.GasTypeCount;

        // Allocate native arrays
        gasDensities = new NativeArray<ushort>[gasCount];
        gasPending = new NativeArray<ushort>[gasCount];

        for (int i = 0; i < gasCount; i++)
        {
            gasDensities[i] = new NativeArray<ushort>(cellCount, Allocator.Persistent);
            gasPending[i] = new NativeArray<ushort>(cellCount, Allocator.Persistent);
        }

        roofedCells = new NativeArray<bool>(cellCount, Allocator.Persistent);
        passabilityGrid = new NativeArray<byte>(cellCount, Allocator.Persistent);

        UpdateMapData();
    }


    void CreateJobInstances()
    {
        int gasCount = GasGridMemoryManager.GasTypeCount;
        dissipationJobs = new GasDissipationJob[gasCount];
        spreadingJobs = new GasSpreadingJob[gasCount];

        for (int i = 0; i < gasCount; i++)
        {
            var gasDef = GasGridMemoryManager.AllGasDefs[i];

            dissipationJobs[i] = new GasDissipationJob
            {
                dissipationRate = gasDef.dissipationAmount / (float)ushort.MaxValue,
                deltaTime = TicksPerProcess,
                gasTypeId = i,
                densities = gasDensities[i],
                pendingChanges = gasPending[i],
                roofedCells = roofedCells,
                mapSizeX = map.Size.x
            };

            spreadingJobs[i] = new GasSpreadingJob
            {
                densities = gasDensities[i],
                pending = gasPending[i],
                passability = passabilityGrid,
                mapSizeX = map.Size.x,
                mapSizeZ = map.Size.z,
                viscosity = gasDef.spreadViscosity,
                minSpreadDensity = (int)gasDef.minSpreadDensity
            };
        }
    }

    void UpdateMapData()
    {
        int cellCount = map.cellIndices.NumGridCells;

        for (int i = 0; i < cellCount; i++)
        {
            var cell = map.cellIndices.IndexToCell(i);

            // Update roof data
            roofedCells[i] = cell.Roofed(map);

            // Update passability (0-255 scale)
            var edifice = cell.GetEdifice(map);
            if (edifice == null)
            {
                passabilityGrid[i] = 255; // Fully passable
            }
            else if (edifice.def.fillPercent >= 1f)
            {
                // Check for doors
                if (edifice is Building_Door door)
                    passabilityGrid[i] = (byte)(door.Open ? 255 : 0);
                else
                    passabilityGrid[i] = 0; // Blocked
            }
            else
            {
                // Partial blockage
                passabilityGrid[i] = (byte)((1f - edifice.def.fillPercent) * 255);
            }
        }
    }

    public void Cleanup()
    {
        // Reset pool when changing maps
        GasGridMemoryManager.ResetPool();
    }

    public void Debug_FillAll()
    {
        throw new System.NotImplementedException();
    }

    public void Debug_PushRadialAdjacent(IntVec3 mouseCell, SpreadingGasDef def)
    {
        throw new System.NotImplementedException();
    }

    public bool AnyGasAt(IntVec3 intVec)
    {
        return grid[intVec.Index(map)].HasAnyGas;
    }
}*/