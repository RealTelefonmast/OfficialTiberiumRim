using System;
using System.Linq.Expressions;

namespace TeleCore.Unsorted;

public static partial class NumericLib<T>
{
    private static Func<T, T, bool> CreateGreaterThan()
    {
        var paramA = Expression.Parameter(typeof(T), "a");
        var paramB = Expression.Parameter(typeof(T), "b");
        var body = Expression.GreaterThan(paramA, paramB);
        return Expression.Lambda<Func<T, T, bool>>(body, paramA, paramB).Compile();
    }

    private static Func<T, T, bool> CreateLessThan()
    {
        var paramA = Expression.Parameter(typeof(T), "a");
        var paramB = Expression.Parameter(typeof(T), "b");
        var body = Expression.LessThan(paramA, paramB);
        return Expression.Lambda<Func<T, T, bool>>(body, paramA, paramB).Compile();
    }

    private static Func<T, T, bool> CreateEqual()
    {
        var paramA = Expression.Parameter(typeof(T), "a");
        var paramB = Expression.Parameter(typeof(T), "b");
        var body = Expression.Equal(paramA, paramB);
        return Expression.Lambda<Func<T, T, bool>>(body, paramA, paramB).Compile();
    }

    private static Func<T, T, bool> CreateNotEqual()
    {
        var paramA = Expression.Parameter(typeof(T), "a");
        var paramB = Expression.Parameter(typeof(T), "b");
        var body = Expression.NotEqual(paramA, paramB);
        return Expression.Lambda<Func<T, T, bool>>(body, paramA, paramB).Compile();
    }

    private static Func<T, T, bool> CreateGreaterThanOrEqual()
    {
        var paramA = Expression.Parameter(typeof(T), "a");
        var paramB = Expression.Parameter(typeof(T), "b");
        var body = Expression.GreaterThanOrEqual(paramA, paramB);
        return Expression.Lambda<Func<T, T, bool>>(body, paramA, paramB).Compile();
    }

    private static Func<T, T, bool> CreateLessThanOrEqual()
    {
        var paramA = Expression.Parameter(typeof(T), "a");
        var paramB = Expression.Parameter(typeof(T), "b");
        var body = Expression.LessThanOrEqual(paramA, paramB);
        return Expression.Lambda<Func<T, T, bool>>(body, paramA, paramB).Compile();
    }

    /// <summary>
    ///     Creates a compiled expression that compares two values and returns:
    ///     -1 if a &lt; b, 0 if a == b, 1 if a &gt; b
    ///     Uses multiple fallback strategies for maximum compatibility.
    /// </summary>
    private static Func<T, T, int> CreateCompareTo()
    {
        var paramA = Expression.Parameter(typeof(T), "a");
        var paramB = Expression.Parameter(typeof(T), "b");
        var type = typeof(T);
        Expression body;

        // Strategy 1: Use comparison operators for value types
        if (type.IsValueType)
            try
            {
                var lessThan = Expression.LessThan(paramA, paramB);
                var greaterThan = Expression.GreaterThan(paramA, paramB);

                body = Expression.Condition(
                    lessThan,
                    Expression.Constant(-1),
                    Expression.Condition(
                        greaterThan,
                        Expression.Constant(1),
                        Expression.Constant(0)
                    )
                );

                return Expression.Lambda<Func<T, T, int>>(body, paramA, paramB).Compile();
            }
            catch
            {
                // Operators not supported, try next strategy
            }

        // Strategy 2: Try IComparable<T>
        var genericComparable = typeof(IComparable<>).MakeGenericType(type);
        if (genericComparable.IsAssignableFrom(type))
        {
            var compareToMethod = type.GetMethod("CompareTo", new[] { type });
            if (compareToMethod != null)
            {
                body = Expression.Call(paramA, compareToMethod, paramB);
                return Expression.Lambda<Func<T, T, int>>(body, paramA, paramB).Compile();
            }
        }

        // Strategy 3: Try non-generic IComparable
        if (typeof(IComparable).IsAssignableFrom(type))
        {
            var compareToMethod = typeof(IComparable).GetMethod("CompareTo");
            var castA = Expression.Convert(paramA, typeof(IComparable));
            var castB = Expression.Convert(paramB, typeof(object));
            body = Expression.Call(castA, compareToMethod!, castB);
            return Expression.Lambda<Func<T, T, int>>(body, paramA, paramB).Compile();
        }

        // Strategy 4: For enums, convert to underlying type and compare
        if (type.IsEnum)
        {
            var underlyingType = Enum.GetUnderlyingType(type);
            var convertedA = Expression.Convert(paramA, underlyingType);
            var convertedB = Expression.Convert(paramB, underlyingType);

            var lessThan = Expression.LessThan(convertedA, convertedB);
            var greaterThan = Expression.GreaterThan(convertedA, convertedB);

            body = Expression.Condition(
                lessThan,
                Expression.Constant(-1),
                Expression.Condition(
                    greaterThan,
                    Expression.Constant(1),
                    Expression.Constant(0)
                )
            );

            return Expression.Lambda<Func<T, T, int>>(body, paramA, paramB).Compile();
        }

        throw new InvalidOperationException(
            $"Type {type.Name} does not support comparison operations and does not implement IComparable<T> or IComparable");
    }
}