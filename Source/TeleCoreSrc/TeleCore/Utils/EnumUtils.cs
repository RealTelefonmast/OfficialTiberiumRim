using System;
using System.Collections.Generic;

namespace TeleCore.Utils;

public static class EnumUtils
{
    public static unsafe bool HasFlag<T>(T enumValue, T flag) where T : unmanaged, Enum
    {
        return enumValue.And(flag).NotZero();
    }
    
    public static IEnumerable<TEnum> GetFlags<TEnum>(this TEnum value)
        where TEnum : Enum
    {
        var valueAsLong = Convert.ToInt64(value);
        
        if (valueAsLong == 0)
            yield break;
    
        foreach (TEnum enumValue in Enum.GetValues(typeof(TEnum)))
        {
            var enumValueAsLong = Convert.ToInt64(enumValue);
            
            if (enumValueAsLong == 0)
                continue;
            
            if ((valueAsLong & enumValueAsLong) == enumValueAsLong)
                yield return enumValue;
        }
    }
    
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