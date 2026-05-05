using System;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace TeleCore.Types.Utils;

public static unsafe class NumericConversionLibrary<TSource, TTarget>
    where TSource : unmanaged
    where TTarget : unmanaged
{
    public static readonly Func<TSource, TTarget> Convert;
    public static readonly Func<TTarget, TSource> Constant;

    private static readonly int TargetSize = sizeof(TTarget);
    private static readonly int SourceSize = sizeof(TSource);


    static NumericConversionLibrary()
    {
        Convert = CreateConvertFunc();
    }

    public static TTarget ReinterpretCast(TSource source)
    {
        if (SourceSize != TargetSize)
            throw new ArgumentException($"Size mismatch: {SourceSize} vs {TargetSize}");

        var result = *(TTarget*)&source;
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TTarget ConvertViaBytes(TSource source)
    {
        if (TargetSize != SourceSize)
            throw new ArgumentException($"Size mismatch: {SourceSize} vs {TargetSize}");

        TTarget result = default;
        Buffer.MemoryCopy(&source, &result, TargetSize, SourceSize);

        return result;
    }

    private static Func<TSource, TTarget> CreateConvertFunc()
    {
        var parameter = Expression.Parameter(typeof(TSource), "source");
        var conversion = Expression.Convert(parameter, typeof(TTarget));
        return Expression.Lambda<Func<TSource, TTarget>>(conversion, parameter).Compile();
    }

    private static Func<TSource, TSource> CreateConstantFunc()
    {
        var paramA = Expression.Parameter(typeof(TSource), "a");
        var convert = Expression.Constant(paramA);
        var lambda = Expression.Lambda<Func<TSource, TSource>>(convert, paramA);
        return lambda.Compile();
    }

    public static TSource ValueOf(TTarget i)
    {
        return Constant(i);
    }
}