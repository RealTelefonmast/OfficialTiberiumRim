using System;
using System.Linq.Expressions;
using HarmonyLib;

namespace TeleCore.Types.Utils;

public static partial class NumericLib<T>
{
    private static Func<T, int, T> CreateLeftShiftFunc()
    {
        var type = typeof(T);
        var paramA = Expression.Parameter(typeof(T), "a");
        var paramB = Expression.Parameter(typeof(int), "b");

        Expression body;
        if (AccessToolsExtensions.IsInteger(type))
        {
            body = Expression.LeftShift(paramA, paramB);
        }
        else if (type.IsEnum)
        {
            var underlyingType = Enum.GetUnderlyingType(type);
            var converted = Expression.Convert(paramA, underlyingType);
            var shift = Expression.LeftShift(converted, paramB);
            body = Expression.Convert(shift, type);
        }
        else
        {
            throw new NotSupportedException($"Left shift not supported for type {type.Name}");
        }

        return Expression.Lambda<Func<T, int, T>>(body, paramA, paramB).Compile();
    }

    private static Func<T, int, T> CreateRightShiftFunc()
    {
        var type = typeof(T);
        var paramA = Expression.Parameter(typeof(T), "a");
        var paramB = Expression.Parameter(typeof(int), "b");

        Expression body;
        if (AccessToolsExtensions.IsInteger(type))
        {
            body = Expression.RightShift(paramA, paramB);
        }
        else if (type.IsEnum)
        {
            var underlyingType = Enum.GetUnderlyingType(type);
            var converted = Expression.Convert(paramA, underlyingType);
            var shift = Expression.RightShift(converted, paramB);
            body = Expression.Convert(shift, type);
        }
        else
        {
            throw new NotSupportedException($"Right shift not supported for type {type.Name}");
        }

        return Expression.Lambda<Func<T, int, T>>(body, paramA, paramB).Compile();
    }

    private static Func<T, T, T> CreateBitwiseAndFunc()
    {
        var paramA = Expression.Parameter(typeof(T), "a");
        var paramB = Expression.Parameter(typeof(T), "b");

        Expression result;
        if (typeof(T).IsEnum)
        {
            var underlyingType = Enum.GetUnderlyingType(typeof(T));
            var convertedA = Expression.Convert(paramA, underlyingType);
            var convertedB = Expression.Convert(paramB, underlyingType);
            var operation = Expression.And(convertedA, convertedB);
            result = Expression.Convert(operation, typeof(T));
        }
        else
        {
            result = Expression.And(paramA, paramB);
        }

        return Expression.Lambda<Func<T, T, T>>(result, paramA, paramB).Compile();
    }

    private static Func<T, T, T> CreateBitwiseOrFunc()
    {
        var paramA = Expression.Parameter(typeof(T), "a");
        var paramB = Expression.Parameter(typeof(T), "b");

        Expression result;
        if (typeof(T).IsEnum)
        {
            var underlyingType = Enum.GetUnderlyingType(typeof(T));
            var convertedA = Expression.Convert(paramA, underlyingType);
            var convertedB = Expression.Convert(paramB, underlyingType);
            var operation = Expression.Or(convertedA, convertedB);
            result = Expression.Convert(operation, typeof(T));
        }
        else
        {
            result = Expression.Or(paramA, paramB);
        }

        return Expression.Lambda<Func<T, T, T>>(result, paramA, paramB).Compile();
    }

    private static Func<T, T, T> CreateBitwiseXorFunc()
    {
        var paramA = Expression.Parameter(typeof(T), "a");
        var paramB = Expression.Parameter(typeof(T), "b");

        Expression result;
        if (typeof(T).IsEnum)
        {
            var underlyingType = Enum.GetUnderlyingType(typeof(T));
            var convertedA = Expression.Convert(paramA, underlyingType);
            var convertedB = Expression.Convert(paramB, underlyingType);
            var operation = Expression.ExclusiveOr(convertedA, convertedB);
            result = Expression.Convert(operation, typeof(T));
        }
        else
        {
            result = Expression.ExclusiveOr(paramA, paramB);
        }

        return Expression.Lambda<Func<T, T, T>>(result, paramA, paramB).Compile();
    }

    private static Func<T, T> CreateBitwiseNotFunc()
    {
        var param = Expression.Parameter(typeof(T), "a");

        Expression result;
        if (typeof(T).IsEnum)
        {
            var underlyingType = Enum.GetUnderlyingType(typeof(T));
            var converted = Expression.Convert(param, underlyingType);
            var operation = Expression.Not(converted);
            result = Expression.Convert(operation, typeof(T));
        }
        else
        {
            result = Expression.Not(param);
        }

        return Expression.Lambda<Func<T, T>>(result, param).Compile();
    }
}