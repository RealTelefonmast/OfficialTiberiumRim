
using System;
using System.Collections.Generic;

namespace TeleCore.Math.MergeDump;

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
}