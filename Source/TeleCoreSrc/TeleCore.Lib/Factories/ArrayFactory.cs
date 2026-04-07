using System;

namespace TeleCore.Lib.Factories;

public static class ArrayFactory
{
    public static TTo[] Factory<TFrom, TTo>(this TFrom[] values, Func<TFrom, TTo> factory, Action<TFrom, TTo> processor)
    {
        var result = new TTo[values.Length];
        for (var i = 0; i < values.Length; i++)
        {
            var val = values[i];
            var res = factory(val);
            processor(val, res);
            result[i] = res;
        }
        return result;
    }
    
    public static TTo[] Factory<TFrom, TTo>(this TFrom[] values, Func<TFrom, TTo> factory)
    {
        var result = new TTo[values.Length];
        for (var i = 0; i < values.Length; i++)
        {
            result[i] = factory(values[i]);
        }
        return result;
    }
}