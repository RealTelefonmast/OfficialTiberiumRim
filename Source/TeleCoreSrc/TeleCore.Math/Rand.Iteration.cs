using System;

namespace TeleCore.Math;

public static class Rand_Iteration
{
    public static void IterateSeeded<T>(T[] arr, ulong seed, Action<T> action)
    {
        for (int i = 0; i < arr.Length; i++)
        {
            var newIndex = PermuteIndex((ulong)i, (ulong)arr.Length, seed);
            action.Invoke(arr[newIndex]);
        }
    }
        
    public static int PermuteIndex(ulong index, ulong size, ulong seed)
    {
        var baseMultiplier = 0x9E3779B97F4A7C15ul ^ seed;
        var multiplier = FindMultiplicativeInverse(baseMultiplier, size);
        return (int)((index * multiplier + seed) % size);
    }

    public static ulong FindMultiplicativeInverse(ulong a, ulong mod)
    {
        long t = 0, newT = 1;
        long r = (long)mod, newR = (long)a;
        while (newR != 0)
        {
            var quotient = r / newR;
            var oldT = t - quotient * newT;
            var oldR = r - quotient * newR;

            t = newT;
            newT = oldT;

            r = newR;
            newR = oldR;
        }

        if (t < 0) 
            t += (long)mod;
        return (ulong)t;
    }
}