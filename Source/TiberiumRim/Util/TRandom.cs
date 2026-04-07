using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace TR.Util;

public static class TRandom
{
    public static float RandValue
    {
        get
        {
            Rand.PushState();
            var value = Rand.Value;
            Rand.PopState();
            return value;
        }
    }

    public static float Range(FloatRange range)
    {
        return Range(range.min, range.max);
    }

    public static float Range(float min, float max)
    {
        if (max <= min) return min;
        Rand.PushState();
        var result = Rand.Value * (max - min) + min;
        Rand.PopState();
        return result;
    }

    public static uint Range(uint min, uint max)
    {
        if (max <= min) return min;
        Rand.PushState();
        var result = min + (uint)Math.Abs(Rand.Int % (max - min));
        Rand.PopState();
        return result;
    }

    public static int Range(IntRange range)
    {
        return Range(range.min, range.max);
    }

    public static int Range(int min, int max)
    {
        if (max <= min) return min;
        Rand.PushState();
        var result = min + Mathf.Abs(Rand.Int % (max - min));
        Rand.PopState();
        return result;
    }

    public static int RangeInclusive(int min, int max)
    {
        if (max <= min) return min;
        return Range(min, max + 1);
    }

    public static bool Chance(float f)
    {
        Rand.PushState();
        var result = Rand.Chance(f);
        Rand.PopState();
        return result;
    }

    //Random Collections
    public static T RandomWeightedElement<T>(this IEnumerable<T> elements, Func<T, float> weightSelector)
    {
        var totalWeight = elements.Sum(weightSelector);
        var randWeight = RandValue * totalWeight;
        var curWeight = 0f;
        foreach (var e in elements)
        {
            var weight = weightSelector(e);
            if (weight <= 0) continue;
            curWeight += weight;
            if (curWeight >= randWeight)
                return e;
        }

        return default;
    }
}