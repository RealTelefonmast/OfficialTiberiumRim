using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace TeleCore.RimWorld.GasGrid;

/// <summary>
///     Stack of gas values for a single cell.
///     Uses dynamically allocated memory from the pool.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public unsafe struct GasCellStack
{
    // Pointer to gas values (allocated from pool)
    private GasCellValue* gasValues;
    private byte gasCount;
    private byte flags;

    // Cached aggregate values

    // Flag definitions
    private const byte FLAG_INITIALIZED = 0x01;
    private const byte FLAG_HAS_GAS = 0x02;
    private const byte FLAG_DIRTY = 0x04;
    private const byte FLAG_BLOCKED = 0x08; // Cell blocked by building

    public readonly bool IsInitialized => (flags & FLAG_INITIALIZED) != 0;
    public readonly bool HasAnyGas => (flags & FLAG_HAS_GAS) != 0;
    public readonly bool IsDirty => (flags & FLAG_DIRTY) != 0;
    public readonly bool IsBlocked => (flags & FLAG_BLOCKED) != 0;
    public readonly int GasCount => gasCount;
    public uint TotalDensity { get; private set; }

    public ushort MaxDensity { get; private set; }

    /// <summary>
    ///     Initialize with pre-allocated memory
    /// </summary>
    public void Initialize(GasCellValue* memory, int gasTypeCount)
    {
        gasValues = memory;
        gasCount = (byte)gasTypeCount;
        flags = FLAG_INITIALIZED;
        TotalDensity = 0;
        MaxDensity = 0;

        // Initialize gas type IDs
        for (var i = 0; i < gasTypeCount; i++) gasValues[i].gasTypeId = (byte)i;
    }

    /// <summary>
    ///     Get gas value by type ID
    /// </summary>
    public readonly ref GasCellValue this[int gasTypeId]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
#if DEBUG
            if (!IsInitialized)
                throw new InvalidOperationException("GasCellStack not initialized");
            if (gasTypeId < 0 || gasTypeId >= gasCount)
                throw new IndexOutOfRangeException($"Gas type {gasTypeId} out of range [0, {gasCount})");
#endif

            return ref gasValues[gasTypeId];
        }
    }

    /// <summary>
    ///     Add gas with overflow distribution
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddGas(int gasTypeId, ushort amount)
    {
        if (!IsInitialized || IsBlocked) return;

        ref var gas = ref gasValues[gasTypeId];
        ushort oldDensity = gas.density;
        gas.AddDensity(amount);

        UpdateCachedValues(oldDensity, gas.density);
        flags |= FLAG_DIRTY;
    }

    /// <summary>
    ///     Update cached aggregate values
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void UpdateCachedValues(ushort oldDensity, ushort newDensity)
    {
        TotalDensity = TotalDensity - oldDensity + newDensity;

        if (newDensity > MaxDensity)
            MaxDensity = newDensity;
        else if (oldDensity == MaxDensity && newDensity < MaxDensity)
            RecalculateMaxDensity();

        if (TotalDensity > 0)
            flags |= FLAG_HAS_GAS;
        else
            ClearFlag(FLAG_HAS_GAS);
    }

    /// <summary>
    ///     Recalculate max density after potential decrease
    /// </summary>
    private void RecalculateMaxDensity()
    {
        ushort max = 0;
        for (var i = 0; i < gasCount; i++)
            if (gasValues[i].density > max)
                max = gasValues[i].density;
        MaxDensity = max;
    }

    /// <summary>
    ///     Apply all pending changes (double-buffering)
    /// </summary>
    public void ApplyPendingChanges()
    {
        if (!IsInitialized) return;

        uint newTotal = 0;
        ushort newMax = 0;

        for (var i = 0; i < gasCount; i++)
        {
            gasValues[i].ApplyPending();
            ushort density = gasValues[i].density;
            newTotal += density;
            if (density > newMax) newMax = density;
        }

        TotalDensity = newTotal;
        MaxDensity = newMax;

        if (newTotal > 0)
            flags |= FLAG_HAS_GAS;
        else
            ClearFlag(FLAG_HAS_GAS);
    }

    /// <summary>
    ///     Mark cell as blocked/unblocked by buildings
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetBlocked(bool blocked)
    {
        if (blocked)
        {
            flags |= FLAG_BLOCKED;
            Clear();
        }
        else
        {
            ClearFlag(FLAG_BLOCKED);
        }
    }

    /// <summary>
    ///     Clear all gas from this cell
    /// </summary>
    public void Clear()
    {
        if (!IsInitialized) return;

        for (var i = 0; i < gasCount; i++)
        {
            gasValues[i].density = 0;
            gasValues[i].pending = 0;
            gasValues[i].overflow = 0;
        }

        TotalDensity = 0;
        MaxDensity = 0;
        ClearFlag(FLAG_HAS_GAS);
    }

    /// <summary>
    ///     Clear dirty flag after processing
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ClearDirty()
    {
        ClearFlag(FLAG_DIRTY);
    }

    #region Flag Helpers

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SetFlag(byte flag)
    {
        flags |= flag;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ClearFlag(byte flag)
    {
        flags &= (byte)~flag;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ToggleFlag(byte flag)
    {
        flags ^= flag;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool HasFlag(byte flag)
    {
        return (flags & flag) != 0;
    }

    #endregion

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();

        sb.Append($"GasCount: {GasCount}");
        sb.Append($"IsInit: {IsInitialized}");
        sb.Append($"IsBlocked: {IsBlocked}");
        sb.Append($"TotalDensity: {TotalDensity}");
        sb.Append($"MaxDensity: {MaxDensity}");
        
        return sb.ToString();
    }
}