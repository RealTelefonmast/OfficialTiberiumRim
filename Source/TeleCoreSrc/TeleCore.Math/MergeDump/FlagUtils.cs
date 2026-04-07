using System;
using System.Runtime.CompilerServices;

namespace TeleCore.Math.MergeDump;

/// <summary>
/// Generic bitwise operations for flag enums using unsafe memory manipulation.
/// Supports any enum type regardless of underlying integral type (byte, int, long, etc.).
/// </summary>
public static class FlagUtils
{
    /// <summary>
    /// Performs bitwise AND operation on two enum values, returning bits that are set in both.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe T And<T>(this T first, T second) where T : unmanaged, Enum
    {
        switch (sizeof(T))
        {
            case 8:
                ulong temp8 = BitwiseUtils.And(*(ulong*)&first, *(ulong*)&second);
                return *(T*)&temp8;
            case 4:
                uint temp4 = BitwiseUtils.And(*(uint*)&first, *(uint*)&second);
                return *(T*)&temp4;
            case 2:
                ushort temp2 = BitwiseUtils.And(*(ushort*)&first, *(ushort*)&second);
                return *(T*)&temp2;
            case 1:
                byte temp1 = BitwiseUtils.And(*(byte*)&first, *(byte*)&second);
                return *(T*)&temp1;
            default:
                throw new NotSupportedException($"Unsupported enum size: {sizeof(T)} bytes");
        }
    }
        
    /// <summary>
    /// Performs bitwise OR operation on two enum values, returning bits that are set in either.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe T Or<T>(this T first, T second) where T : unmanaged, Enum
    {
        switch (sizeof(T))
        {
            case 8:
                ulong temp8 = BitwiseUtils.Or(*(ulong*)&first, *(ulong*)&second);
                return *(T*)&temp8;
            case 4:
                uint temp4 = BitwiseUtils.Or(*(uint*)&first, *(uint*)&second);
                return *(T*)&temp4;
            case 2:
                ushort temp2 = BitwiseUtils.Or(*(ushort*)&first, *(ushort*)&second);
                return *(T*)&temp2;
            case 1:
                byte temp1 = BitwiseUtils.Or(*(byte*)&first, *(byte*)&second);
                return *(T*)&temp1;
            default:
                throw new NotSupportedException($"Unsupported enum size: {sizeof(T)} bytes");
        }
    }
        
    /// <summary>
    /// Performs bitwise XOR operation on two enum values, returning bits that are set in either but not both.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe T Xor<T>(this T first, T second) where T : unmanaged, Enum
    {
        switch (sizeof(T))
        {
            case 8:
                ulong temp8 = BitwiseUtils.Xor(*(ulong*)&first, *(ulong*)&second);
                return *(T*)&temp8;
            case 4:
                uint temp4 = BitwiseUtils.Xor(*(uint*)&first, *(uint*)&second);
                return *(T*)&temp4;
            case 2:
                ushort temp2 = BitwiseUtils.Xor(*(ushort*)&first, *(ushort*)&second);
                return *(T*)&temp2;
            case 1:
                byte temp1 = BitwiseUtils.Xor(*(byte*)&first, *(byte*)&second);
                return *(T*)&temp1;
            default:
                throw new NotSupportedException($"Unsupported enum size: {sizeof(T)} bytes");
        }
    }
        
    /// <summary>
    /// Performs bitwise NOT (complement) operation on an enum value.
    /// </summary>
    /// <remarks>
    /// May produce values with undefined bits set that aren't part of the enum definition.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe T Not<T>(this T value) where T : unmanaged, Enum
    {
        switch (sizeof(T))
        {
            case 8:
                ulong temp8 = BitwiseUtils.Not(*(ulong*)&value);
                return *(T*)&temp8;
            case 4:
                uint temp4 = BitwiseUtils.Not(*(uint*)&value);
                return *(T*)&temp4;
            case 2:
                ushort temp2 = BitwiseUtils.Not(*(ushort*)&value);
                return *(T*)&temp2;
            case 1:
                byte temp1 = BitwiseUtils.Not(*(byte*)&value);
                return *(T*)&temp1;
            default:
                throw new NotSupportedException($"Unsupported enum size: {sizeof(T)} bytes");
        }
    }

    /// <summary>
    /// Compares two enum values for bitwise equality using memory-level comparison.
    /// More reliable than == operator for generic enum operations.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe bool Is<T>(this T first, T second) where T : unmanaged, Enum
    {
        return sizeof(T) switch
        {
            8 => BitwiseUtils.Is(*(ulong*)&first, *(ulong*)&second),
            4 => BitwiseUtils.Is(*(uint*)&first, *(uint*)&second),
            2 => BitwiseUtils.Is(*(ushort*)&first, *(ushort*)&second),
            1 => BitwiseUtils.Is(*(byte*)&first, *(byte*)&second),
            _ => throw new NotSupportedException($"Unsupported enum size: {sizeof(T)} bytes")
        };
    }
        
    /// <summary>
    ///  Compares whether an enum has any bits set other than those in the specified flag.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe bool IsNot<T>(this T first, T second) where T : unmanaged, Enum
    {
        return sizeof(T) switch
        {
            8 => BitwiseUtils.IsNot(*(ulong*)&first, *(ulong*)&second),
            4 => BitwiseUtils.IsNot(*(uint*)&first, *(uint*)&second),
            2 => BitwiseUtils.IsNot(*(ushort*)&first, *(ushort*)&second),
            1 => BitwiseUtils.IsNot(*(byte*)&first, *(byte*)&second),
            _ => throw new NotSupportedException($"Unsupported enum size: {sizeof(T)} bytes")
        };
    }
        
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe bool NotZero<T>(this T value) where T : unmanaged, Enum
    {
        return sizeof(T) switch
        {
            8 => BitwiseUtils.IsNotZero(*(ulong*)&value),
            4 => BitwiseUtils.IsNotZero(*(uint*)&value),
            2 => BitwiseUtils.IsNotZero(*(ushort*)&value),
            1 => BitwiseUtils.IsNotZero(*(byte*)&value),
            _ => throw new NotSupportedException($"Unsupported enum size: {sizeof(T)} bytes")
        };
    }

    /// <summary>
    /// Tests if an enum value has any bit of the flag set.
    /// Equivalent to (value &amp; flag) != 0
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe bool HasBit<T>(this T value, T flag) where T : unmanaged, Enum
    {
        return sizeof(T) switch
        {
            8 => BitwiseUtils.HasBit(*(ulong*)&value, *(ulong*)&flag),
            4 => BitwiseUtils.HasBit(*(uint*)&value, *(uint*)&flag),
            2 => BitwiseUtils.HasBit(*(ushort*)&value, *(ushort*)&flag),
            1 => BitwiseUtils.HasBit(*(byte*)&value, *(byte*)&flag),
            _ => throw new NotSupportedException($"Unsupported enum size: {sizeof(T)} bytes")
        };
    }
}
    
public static class BitwiseUtils
{
    // For Burst compatibility, we need specific implementations for common types
    // rather than fully generic methods with sizeof(T)

    #region Byte Operations
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte And(byte first, byte second) => (byte)(first & second);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte Or(byte first, byte second) => (byte)(first | second);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte Xor(byte first, byte second) => (byte)(first ^ second);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte Not(byte value) => (byte)~value;
        
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HasBit(byte value, byte flag) => (value & flag) != 0;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Is(byte first, byte second) => (first & second) == second;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNot(byte first, byte second) => (first & ~second) != 0;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsZero(byte value) => value == 0;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNotZero(byte value) => value != 0;
    #endregion

    #region UShort Operations
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ushort And(ushort first, ushort second) => (ushort)(first & second);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ushort Or(ushort first, ushort second) => (ushort)(first | second);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ushort Xor(ushort first, ushort second) => (ushort)(first ^ second);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ushort Not(ushort value) => (ushort)~value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HasBit(ushort value, ushort flag) => (value & flag) != 0;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Is(ushort first, ushort second) => (first & second) == second;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNot(ushort first, ushort second) => (first & ~second) != 0;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsZero(ushort value) => value == 0;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNotZero(ushort value) => value != 0;
    #endregion

    #region UInt Operations
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint And(uint first, uint second) => first & second;
        
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint Or(uint first, uint second) => first | second;
        
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint Xor(uint first, uint second) => first ^ second;
        
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint Not(uint value) => ~value;
        
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HasBit(uint value, uint flag) => (value & flag) != 0;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Is(uint first, uint second) => (first & second) == second;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNot(uint first, uint second) => (first & ~second) != 0;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsZero(uint value) => value == 0;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNotZero(uint value) => value != 0;
    #endregion

    #region ULong Operations
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong And(ulong first, ulong second) => first & second;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong Or(ulong first, ulong second) => first | second;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong Xor(ulong first, ulong second) => first ^ second;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong Not(ulong value) => ~value;
        
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HasBit(ulong value, ulong flag) => (value & flag) != 0;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Is(ulong first, ulong second) => (first & second) == second;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNot(ulong first, ulong second) => (first & ~second) != 0;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsZero(ulong value) => value == 0;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNotZero(ulong value) => value != 0;
    #endregion

    #region Int Operations (for signed enums)
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int And(int first, int second) => first & second;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Or(int first, int second) => first | second;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Xor(int first, int second) => first ^ second;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Not(int value) => ~value;
        
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HasBit(int value, int flag) => (value & flag) != 0;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Is(int first, int second) => (first & second) == second;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNot(int first, int second) => (first & ~second) != 0;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsZero(int value) => value == 0;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNotZero(int value) => value != 0;
    #endregion
}