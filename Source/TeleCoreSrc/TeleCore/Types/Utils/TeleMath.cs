using System;
using System.Collections.Generic;

namespace TeleCore.Unsorted;

public static class TeleMath<T> where T : unmanaged
{
    public static T Zero = default;
}

public static class TeleMath
{
    public static T Clamp<T>(T value, T min, T max) where T : unmanaged
    {
        return NumericLibrary<T>.Clamp(value, min, max);
    }

    public static T Min<T>(T a, T b) where T : unmanaged
    {
        return NumericLibrary<T>.Min(a, b);
    }

    public static T Max<T>(T a, T b) where T : unmanaged
    {
        return NumericLibrary<T>.Max(a, b);
    }

    public static T Sum<T>(this IEnumerable<T> values) where T : unmanaged
    {
        return NumericLibrary<T>.Sum(values);
    }

    public static T Abs<T>(Numeric<T> value) where T : unmanaged
    {
        return NumericLibrary<T>.Abs(value);
    }

    public static T Round<T>(Numeric<T> value, int decimals) where T : unmanaged
    {
        return NumericLibrary<T>.Round(value, decimals);
    }

    public static unsafe bool IsZero<T>(this T value) where T : unmanaged
    {
        var size = sizeof(T);
        var ptr = &value;

        switch (size)
        {
            case 1:
                return *(byte*)ptr == 0;
            case 2:
                return *(ushort*)ptr == 0;
            case 4:
                return *(uint*)ptr == 0;
            case 8:
                return *(ulong*)ptr == 0;
            default:
                var bytePtr = (byte*)ptr;
                for (var i = 0; i < size; i++)
                    if (bytePtr[i] != 0)
                        return false;
                return true;
        }
    }

    public static unsafe T And<T>(this T first, T second) where T : unmanaged
    {
        var size = sizeof(T);
        var ptr = &first;
        var ptr2 = &second;

        switch (size)
        {
            case 1:
                var bt = *(byte*)ptr;
                var btf = *(byte*)ptr2;
                var bres = bt & btf;
                return *(T*)&bres;
            case 2:
                var us = *(ushort*)ptr;
                var usf = *(ushort*)ptr2;
                var usres = us & usf;
                return *(T*)&usres;
            case 4:
                var ui = *(uint*)ptr;
                var uif = *(uint*)ptr2;
                var uires = ui & uif;
                return *(T*)&uires;
            case 8:
                var ul = *(ulong*)ptr;
                var ulf = *(ulong*)ptr2;
                var ulres = ul & ulf;
                return *(T*)&ulres;
        }

        throw new ArgumentException();
    }

    public static unsafe T Or<T>(this T first, T second) where T : unmanaged
    {
        var size = sizeof(T);
        var ptr = &first;
        var ptr2 = &second;

        switch (size)
        {
            case 1:
                var bt = *(byte*)ptr;
                var btf = *(byte*)ptr2;
                var bres = bt | btf;
                return *(T*)&bres;
            case 2:
                var us = *(ushort*)ptr;
                var usf = *(ushort*)ptr2;
                var usres = us | usf;
                return *(T*)&usres;
            case 4:
                var ui = *(uint*)ptr;
                var uif = *(uint*)ptr2;
                var uires = ui | uif;
                return *(T*)&uires;
            case 8:
                var ul = *(ulong*)ptr;
                var ulf = *(ulong*)ptr2;
                var ulres = ul | ulf;
                return *(T*)&ulres;
        }

        throw new ArgumentException();
    }
}