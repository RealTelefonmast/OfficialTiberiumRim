using System.Runtime.CompilerServices;
using TeleCore.Defs;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Verse;

namespace TeleCore.Unsorted;

/// <summary>
///     Manages static gas metadata and memory allocation for the gas grid system.
///     Initialized once at game startup after all mods are loaded.
/// </summary>
[StaticConstructorOnStartup]
public static unsafe class GasGridMemoryManager
{
    private static bool _initialized;

    // Memory pools for stack allocations
    private static NativeArray<GasCellValue> _stackMemoryPool;
    private static int _poolOffset;
    private static readonly object PoolLock = new();

    public static int GasTypeCount { get; private set; }

    public static SpreadingGasDef[] AllGasDefs { get; private set; }
    
    public static bool IsInitialized => _initialized;

    static GasGridMemoryManager()
    {
        Initialize();
        TLog.Message($"Loaded GasGridMemoryManager with {GasTypeCount} gasses!");
    }
    
    // Called after all defs are loaded
    internal static void Initialize()
    {
        if (_initialized) return;

        AllGasDefs = DefDatabase<SpreadingGasDef>.AllDefsListForReading.ToArray();
        GasTypeCount = AllGasDefs.Length;

        // Pre-allocate a large pool for gas cell stacks
        // Assuming max 300x300 map = 90,000 cells
        var maxCells = 90000;
        _stackMemoryPool = new NativeArray<GasCellValue>(
            maxCells * GasTypeCount,
            Allocator.Persistent
        );
        _poolOffset = 0;

        _initialized = true;

        Log.Message($"GasGridMemoryManager initialized with {GasTypeCount} gas types");
    }

    /// <summary>
    ///     Allocate memory for a new gas cell stack from the pool
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static GasCellValue* AllocateStackMemory()
    {
        lock (PoolLock)
        {
            if (_poolOffset + GasTypeCount > _stackMemoryPool.Length)
            {
                Log.Error("Gas stack memory pool exhausted!");
                return null;
            }

            var ptr = (GasCellValue*)_stackMemoryPool.GetUnsafePtr() + _poolOffset;
            _poolOffset += GasTypeCount;

            // Initialize to zero
            UnsafeUtility.MemClear(ptr, GasTypeCount * sizeof(GasCellValue));

            return ptr;
        }
    }

    /// <summary>
    ///     Create a new initialized gas cell stack
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static GasCellStack CreateStack()
    {
        var stack = new GasCellStack();
        stack.Initialize(AllocateStackMemory(), GasTypeCount);
        return stack;
    }

    /// <summary>
    ///     Reset the memory pool (call when changing maps)
    /// </summary>
    public static void ResetPool()
    {
        lock (PoolLock)
        {
            _poolOffset = 0;
            // Clear all memory
            UnsafeUtility.MemClear(
                _stackMemoryPool.GetUnsafePtr(),
                _stackMemoryPool.Length * sizeof(GasCellValue)
            );
        }
    }

    public static void Cleanup()
    {
        if (_stackMemoryPool.IsCreated)
            _stackMemoryPool.Dispose();
        _initialized = false;
    }
}