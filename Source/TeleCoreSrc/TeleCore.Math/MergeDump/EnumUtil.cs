using System;
using System.Collections.Generic;

namespace TeleCore.Math.MergeDump;

public static class EnumUtil
{
    public static TEnum[] GetFlags<TEnum>(this TEnum input)
        where TEnum : unmanaged
    {
        var numericInput = new Numeric<TEnum>(input);
        var bits = numericInput.Bits;
        Span<TEnum> buffer = stackalloc TEnum[bits];
        var count = 0;

        for (var bit = 0; bit < bits; bit++)
        {
            var flag = Numeric<TEnum>.One << bit;
            var check = numericInput & flag; //NumericLibrary<TEnum>.BitwiseAnd(numericInput.Value, flag);
            var check2 = new Numeric<TEnum>(check);
            var hasFlag = check2 == flag;
            if (hasFlag) 
                buffer[count++] = flag;
        }

        var result = new TEnum[count];
        for (var i = 0; i < count; i++) 
            result[i] = buffer[i];
        return result;
    }

    public static TEnum[] GetFlagsWithList<TEnum>(TEnum input) where TEnum : unmanaged
    {
        var numericInput = new Numeric<TEnum>(input);
        var bits = numericInput.Bits;
        var result = new List<TEnum>(bits);
        for (var bit = 0; bit < bits; bit++)
        {
            var flag = Numeric<TEnum>.One << bit;
            var check = numericInput & flag;
            var check2 = new Numeric<TEnum>(check);
            var hasFlag = check2 == flag;
            if (hasFlag)
                result.Add(flag);
        }

        return result.ToArray();
    }
}