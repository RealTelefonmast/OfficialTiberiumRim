using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Mathematics;

namespace TeleCore.RimWorld.GasGrid;

/// <summary>
///     Single gas value in a cell. Compact 8-byte structure.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct GasCellValue
{
    public ushort density; // Current density (0-65535)
    public ushort pending; // Pending changes for double-buffering
    public ushort overflow; // Overflow amount that couldn't fit
    public byte gasTypeId; // Which gas type this represents
    public byte flags; // Various flags

    // Flag definitions
    public const byte FLAG_SPREADING = 0x01;
    public const byte FLAG_DISSIPATING = 0x02;
    public const byte FLAG_BLOCKED = 0x04;

    public const ushort MaxDensity = ushort.MaxValue;

    public readonly bool CanSpread
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => density >= GetSpreadThreshold();
    }

    public readonly bool HasGas
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => density > 0;
    }

    public readonly float DensityNormalized
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => density / (float)MaxDensity;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private readonly ushort GetSpreadThreshold()
    {
        // Could cache this in the memory manager if needed
        var def = GasGridMemoryManager.AllGasDefs[gasTypeId];
        return (ushort)def.minSpreadDensity;
    }

    /// <summary>
    ///     Add density with overflow handling
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddDensity(int amount)
    {
        int newTotal = density + amount;

        if (newTotal > MaxDensity)
        {
            overflow = (ushort)math.min(overflow + (newTotal - MaxDensity), MaxDensity);
            density = MaxDensity;
        }
        else if (newTotal < 0)
        {
            density = 0;
        }
        else
        {
            density = (ushort)newTotal;
        }
    }

    /// <summary>
    ///     Apply pending changes (for double-buffering)
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ApplyPending()
    {
        if (pending == 0) return;

        AddDensity(pending);
        pending = 0;
    }
}