using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace TeleCore.Math;

public static partial class NumericLib<T>
{
    private static Func<IEnumerable<T>, T> CreateSumFunc()
    {
        var itemsExpr = Expression.Parameter(typeof(IEnumerable<T>), "items");
        var enumItemType = typeof(IEnumerable<>).MakeGenericType(typeof(T));

        // Find the appropriate Sum method from Enumerable
        var sumMethod = typeof(Enumerable)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(m => m.Name == "Sum")
            .Where(m => m.ReturnType == typeof(T))
            .FirstOrDefault(m =>
                m.GetParameters().Length == 1
                && m.GetParameters()[0].ParameterType == enumItemType);

        if (sumMethod == null)
            throw new NotSupportedException($"No Sum method found for type {typeof(T).Name}");

        var body = Expression.Call(null, sumMethod, itemsExpr);
        return Expression.Lambda<Func<IEnumerable<T>, T>>(body, itemsExpr).Compile();
    }

    private static Func<T, T, T, T> CreateClampFunc()
    {
        var value = Expression.Parameter(typeof(T), "value");
        var min = Expression.Parameter(typeof(T), "min");
        var max = Expression.Parameter(typeof(T), "max");

        var body = Expression.Condition(
            Expression.LessThan(value, min),
            min,
            Expression.Condition(
                Expression.GreaterThan(value, max),
                max,
                value));

        return Expression.Lambda<Func<T, T, T, T>>(body, value, min, max).Compile();
    }

    private static Func<T, int, T> CreateRoundFunc()
    {
        // For non-floating point types, just return the value unchanged
        if (typeof(T) != typeof(double) && typeof(T) != typeof(float) && typeof(T) != typeof(decimal))
            return (toRound, _) => toRound;

        var value = Expression.Parameter(typeof(T), "value");
        var decimals = Expression.Parameter(typeof(int), "decimals");

        if (typeof(T) == typeof(float))
        {
            // Float needs conversion to double for Math.Round
            var converted = Expression.Convert(value, typeof(double));
            var roundMethod = typeof(System.Math).GetMethod("Round", new[] { typeof(double), typeof(int) })!;
            var body = Expression.Call(roundMethod, converted, decimals);
            var roundConvert = Expression.Convert(body, typeof(float));
            return Expression.Lambda<Func<T, int, T>>(roundConvert, value, decimals).Compile();
        }
        else
        {
            var roundMethod = typeof(System.Math).GetMethod("Round", new[] { typeof(T), typeof(int) })!;
            var body = Expression.Call(roundMethod, value, decimals);
            return Expression.Lambda<Func<T, int, T>>(body, value, decimals).Compile();
        }
    }

    private static Func<T, T, T> CreateMinFunc()
    {
        var paramA = Expression.Parameter(typeof(T), "a");
        var paramB = Expression.Parameter(typeof(T), "b");
        var body = Expression.Condition(
            Expression.LessThan(paramA, paramB),
            paramA,
            paramB);
        return Expression.Lambda<Func<T, T, T>>(body, paramA, paramB).Compile();
    }

    private static Func<T, T, T> CreateMaxFunc()
    {
        var paramA = Expression.Parameter(typeof(T), "a");
        var paramB = Expression.Parameter(typeof(T), "b");
        var body = Expression.Condition(
            Expression.GreaterThan(paramA, paramB),
            paramA,
            paramB);
        return Expression.Lambda<Func<T, T, T>>(body, paramA, paramB).Compile();
    }

    private static Func<T, T> CreateAbsFunc()
    {
        var valueParam = Expression.Parameter(typeof(T), "value");
        var zeroConst = Expression.Constant(0);
        var zero = Expression.Convert(zeroConst, typeof(T));

        var body = Expression.Condition(
            Expression.LessThan(valueParam, zero),
            Expression.Negate(valueParam),
            valueParam);

        return Expression.Lambda<Func<T, T>>(body, valueParam).Compile();
    }
}
