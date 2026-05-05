using System;
using System.Linq.Expressions;

namespace TeleCore.Types.Utils;

public static partial class NumericLib<T>
{
    private static Func<T, T, T> CreateAddFunc()
    {
        var paramA = Expression.Parameter(typeof(T), "a");
        var paramB = Expression.Parameter(typeof(T), "b");
        var body = Expression.Add(paramA, paramB);
        return Expression.Lambda<Func<T, T, T>>(body, paramA, paramB).Compile();
    }

    private static Func<T, T, T> CreateSubtractFunc()
    {
        var paramA = Expression.Parameter(typeof(T), "a");
        var paramB = Expression.Parameter(typeof(T), "b");
        var body = Expression.Subtract(paramA, paramB);
        return Expression.Lambda<Func<T, T, T>>(body, paramA, paramB).Compile();
    }

    private static Func<T, T, T> CreateMultiplicationFunc()
    {
        var paramA = Expression.Parameter(typeof(T), "a");
        var paramB = Expression.Parameter(typeof(T), "b");
        var body = Expression.Multiply(paramA, paramB);
        return Expression.Lambda<Func<T, T, T>>(body, paramA, paramB).Compile();
    }

    private static Func<T, T, T> CreateDivisionFunc()
    {
        var paramA = Expression.Parameter(typeof(T), "a");
        var paramB = Expression.Parameter(typeof(T), "b");
        var body = Expression.Divide(paramA, paramB);
        return Expression.Lambda<Func<T, T, T>>(body, paramA, paramB).Compile();
    }
}