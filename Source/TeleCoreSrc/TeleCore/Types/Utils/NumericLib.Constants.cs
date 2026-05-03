using System;
using System.Linq.Expressions;

namespace TeleCore.Unsorted;

public static partial class NumericLib<T>
{
    private static Func<T> CreateZeroGetter()
    {
        var zeroConstant = Expression.Constant(0);
        var zero = Expression.Convert(zeroConstant, typeof(T));
        return Expression.Lambda<Func<T>>(zero).Compile();
    }

    private static Func<T> CreateOneGetter()
    {
        var oneConst = Expression.Constant(1);
        var one = Expression.Convert(oneConst, typeof(T));
        return Expression.Lambda<Func<T>>(one).Compile();
    }

    private static Func<T> CreateNegativeOneGetter()
    {
        var negOneConst = Expression.Constant(-1);
        var negOne = Expression.Convert(negOneConst, typeof(T));
        return Expression.Lambda<Func<T>>(negOne).Compile();
    }

    private static Func<T> CreateNaNGetter()
    {
        ConstantExpression constant;

        if (typeof(T) == typeof(float))
            constant = Expression.Constant(float.NaN);
        else if (typeof(T) == typeof(double))
            constant = Expression.Constant(double.NaN);
        else
            constant = Expression.Constant(0); // For non-floating point types

        var nan = Expression.Convert(constant, typeof(T));
        return Expression.Lambda<Func<T>>(nan).Compile();
    }

    private static Func<T> CreateEpsilonGetter()
    {
        object epsilon;

        if (typeof(T) == typeof(float))
            epsilon = float.Epsilon;
        else if (typeof(T) == typeof(double))
            epsilon = double.Epsilon;
        else if (typeof(T) == typeof(decimal))
            epsilon = (decimal)double.Epsilon;
        else
            epsilon = 0; // For integer types

        var constant = Expression.Constant(epsilon, typeof(T));
        var body = Expression.Convert(constant, typeof(T));
        return Expression.Lambda<Func<T>>(body).Compile();
    }
}