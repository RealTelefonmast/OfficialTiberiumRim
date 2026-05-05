using System;
using System.Linq.Expressions;

namespace TeleCore.Types.Utils;

/// <summary>
///     Provides type conversion functionality between numeric types using expression compilation.
/// </summary>
public static class NumericConversionLib<TFrom, TTo>
    where TFrom : unmanaged
    where TTo : unmanaged
{
    public static readonly Func<TFrom, TTo>? Convert;
    public static readonly Func<TTo, TFrom>? ConvertBack;

    static NumericConversionLib()
    {
        Convert = TryCreateConvert();
        ConvertBack = TryCreateConvertBack();
    }

    private static Func<TFrom, TTo>? TryCreateConvert()
    {
        try
        {
            var param = Expression.Parameter(typeof(TFrom), "value");
            var convert = Expression.Convert(param, typeof(TTo));
            return Expression.Lambda<Func<TFrom, TTo>>(convert, param).Compile();
        }
        catch
        {
            return null;
        }
    }

    private static Func<TTo, TFrom>? TryCreateConvertBack()
    {
        try
        {
            var param = Expression.Parameter(typeof(TTo), "value");
            var convert = Expression.Convert(param, typeof(TFrom));
            return Expression.Lambda<Func<TTo, TFrom>>(convert, param).Compile();
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    ///     Creates a value of type TFrom from a value of type TTo.
    /// </summary>
    public static TFrom ValueOf(TTo value)
    {
        if (ConvertBack == null)
            throw new InvalidOperationException($"Cannot convert from {typeof(TTo).Name} to {typeof(TFrom).Name}");
        return ConvertBack(value);
    }
}